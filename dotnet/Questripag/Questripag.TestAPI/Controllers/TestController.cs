using Microsoft.AspNetCore.Mvc;

namespace Questripag.TestAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TestController : ControllerBase
    {
        [HttpGet]
        public int Get(Query<IQueryOptions> query)
        {
            throw new NotImplementedException();
        }
    }

    public interface IQueryOptions
    {
        public string Prop1 { get; set; }
        public string Prop2 { get; set; }
    }
}
