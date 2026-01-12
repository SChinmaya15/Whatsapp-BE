using backend.Config;
using backend.Infrastructure;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace backend.Controllers
{
    [ApiController]
    [Route("webhook")]
    public class WebhookController : ControllerBase
    {
        private readonly MongoRepo _repo;
        private readonly WebhookService _service;
        private readonly WhatsAppOptions _whatsAppOptions;

        public WebhookController(
            MongoRepo repo,
            WebhookService service,
            IOptions<WhatsAppOptions> options)
        {
            _repo = repo;
            _service = service;
            _whatsAppOptions = options.Value;
        }

        // --------------------------------------------------
        // GET /webhook  (WhatsApp Verification)
        // --------------------------------------------------
        [HttpGet]
        public IActionResult Verify(
            [FromQuery(Name = "hub.mode")] string mode,
            [FromQuery(Name = "hub.verify_token")] string token,
            [FromQuery(Name = "hub.challenge")] string challenge)
        {
            if (string.IsNullOrEmpty(mode) ||
                string.IsNullOrEmpty(token) ||
                string.IsNullOrEmpty(challenge))
            {
                return BadRequest("Missing verification parameters");
            }

            if (mode == "subscribe" && token == _whatsAppOptions.VerifyToken)
            {
                // IMPORTANT: return plain text challenge
                return Ok(challenge);
            }

            return Forbid();
        }

        // --------------------------------------------------
        // POST /webhook  (Incoming Events)
        // --------------------------------------------------
        [HttpPost]
        public async Task<IActionResult> Receive()
        {
            string body;

            using (var reader = new StreamReader(Request.Body))
            {
                body = await reader.ReadToEndAsync();
            }

            if (string.IsNullOrWhiteSpace(body))
            {
                // WhatsApp may send empty pings
                return Ok();
            }

            JsonDocument payload;
            try
            {
                payload = JsonDocument.Parse(body);
            }
            catch (JsonException)
            {
                // Never throw for invalid JSON
                return BadRequest("Invalid JSON payload");
            }

            // ------------------ ENTRY ------------------
            if (!payload.RootElement.TryGetProperty("entry", out var entries) ||
                entries.ValueKind != JsonValueKind.Array ||
                entries.GetArrayLength() == 0)
            {
                return Ok("No entry data");
            }

            var entry = entries[0];

            // ------------------ CHANGES ------------------
            if (!entry.TryGetProperty("changes", out var changes) ||
                changes.ValueKind != JsonValueKind.Array ||
                changes.GetArrayLength() == 0)
            {
                return Ok("No changes data");
            }

            var change = changes[0];

            // ------------------ VALUE ------------------
            if (!change.TryGetProperty("value", out var value))
            {
                return Ok("No value data");
            }

            // ------------------ MESSAGES ------------------
            if (value.TryGetProperty("messages", out var messages) &&
                messages.ValueKind == JsonValueKind.Array)
            {
                try
                {
                    await _service.SaveMessage(messages);
                }
                catch (Exception ex)
                {
                    // Log but DO NOT break webhook contract
                    Console.WriteLine("SaveMessage failed: " + ex.Message);
                }
            }

            // ------------------ STATUSES ------------------
            if (value.TryGetProperty("statuses", out var statuses) &&
                statuses.ValueKind == JsonValueKind.Array)
            {
                foreach (var status in statuses.EnumerateArray())
                {
                    status.TryGetProperty("id", out var id);
                    status.TryGetProperty("status", out var state);

                    Console.WriteLine(
                        $"Status update: {id.GetString()} => {state.GetString()}"
                    );
                }
            }

            // Always respond 200 quickly
            return Ok();
        }
    }
}
