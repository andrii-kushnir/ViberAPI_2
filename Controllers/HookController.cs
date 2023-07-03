using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace ViberAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HookController : ControllerBase
    {
        private const string webhookPath = "https://chatapi.viber.com/pa/set_webhook";

        [HttpPost]
        public async Task<ActionResult> Post([FromBody]  string webhook)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("X-Viber-Auth-Token", Program.authToken);
                var requestJson = "{\"url\":\"" + webhook + "/main\",\"event_types\":[\"delivered\",\"seen\",\"failed\",\"subscribed\",\"unsubscribed\",\"conversation_started\"],\"send_name\": true,\"send_photo\": true}";
                var response = await httpClient.PostAsync(webhookPath, new StringContent(requestJson));
            }
            return Ok();
        }
    }
}
