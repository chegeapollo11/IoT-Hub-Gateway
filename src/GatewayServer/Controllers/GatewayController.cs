﻿using Dapr.Client;
using GatewayServer.Models;
using GatewayServer.Services;
using GatewayServer.Utils;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GatewayServer.Controllers
{
    [Route("api/[controller]")]
    public class GatewayController : Controller
    {
        private RunnerConfiguration runnerConfigs;
        private DaprClient daprClient;
        private RunnerStats runnerStats;

        public GatewayController(RunnerConfiguration runnerConfigs, RunnerStats stats, DaprClient daprClient)
        {
            this.runnerConfigs = runnerConfigs;
            this.daprClient = daprClient;
            this.runnerStats = stats;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return await Task.FromResult<IActionResult>(new ObjectResult(new { Version = "1.0", Cache = runnerConfigs.IsCacheEnabled }));
        }

        /// <summary>
        /// Sends a message for the given device
        /// </summary>
        /// <param name="deviceId">Device identifier</param>
        /// <param name="payload">Payload (JSON format)</param>
        /// <returns></returns>
        [HttpPost("{deviceId}")]
        public async Task<IActionResult> Send(string deviceId, [FromBody] dynamic payload)
        {
            try
            {
                if (string.IsNullOrEmpty(deviceId))
                    return BadRequest(new { error = "Missing deviceId" });

                if (payload is null)
                    return BadRequest(new { error = "Missing payload" });

                var deviceFactory = new DeviceFactory(deviceId, runnerConfigs, daprClient, runnerStats);
                await deviceFactory.Device.Sender.SendMessageAsync(payload.ToString(), runnerStats, CancellationToken.None);

                //var sasToken = this.ControllerContext.HttpContext.Request.Headers[Constants.SasTokenHeaderName].ToString();
                //if (!string.IsNullOrEmpty(sasToken))
                //{
                //    var tokenExpirationDate = ResolveTokenExpiration(sasToken);
                //    if (!tokenExpirationDate.HasValue)
                //        tokenExpirationDate = DateTime.UtcNow.AddMinutes(20);

                //    await gatewayService.SendDeviceToCloudMessageByToken(deviceId, payload.ToString(), sasToken, tokenExpirationDate.Value);
                //}
                //else
                //{
                //    if (!this.options.SharedAccessPolicyKeyEnabled)
                //        return BadRequest(new { error = "Shared access is not enabled" });
                //    await gatewayService.SendDeviceToCloudMessageBySharedAccess(deviceId, payload.ToString());
                //}

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message, Stack = ex.StackTrace });
                //return new StatusCodeResult(500);
            }
        }
    }
}
