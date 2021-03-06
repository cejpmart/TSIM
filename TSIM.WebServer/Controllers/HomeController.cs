using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TSIM.WebServer.Models;

namespace TSIM.WebServer.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
        }

        public IActionResult DisplayViewAsImage()
        {
            var sim = Program.uglyGlobalSimulation;

            lock (sim)
            {
                var filename = "/tmp/tmp.png";
                var w = 1600;
                var h = 1000;
                var scale = 0.150;          // TODO: automatically determine boundaries of view
                var fontSize = 9;
                GraphicsOutput.RenderPng(sim.CoordSpace, sim.Network, sim.Units, Program.uglyGlobalTCSS, filename, w, h, scale, fontSize);

                byte[] filedata = System.IO.File.ReadAllBytes(filename);
                string contentType = "image/png";

                return File(filedata, contentType);
            }
        }

        // [HttpPost]
        // public void UnitSpeedSet([FromBody] UnitSpeedSetOptions options)
        // {
        //     var sim = Program.uglyGlobalSimulation;

        //     lock (sim)
        //     {
        //         sim.Units.SetUnitSpeed(options.unit, Math.Max(0, options.speed));
        //     }
        // }

        /*
        public class UnitSpeedSetOptions
        {
            public int unit { get; set; }
            public float speed { get; set; }
        }
        */
    }
}
