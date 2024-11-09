using Microsoft.AspNetCore.Mvc;

namespace DotNetDefaultWebApi.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class HostController : ControllerBase
	{
		[Route("/Name")]
		[HttpGet]
		public string Get()
		{
			string hostname = System.Net.Dns.GetHostName();
			return hostname;
		}
	}
}
