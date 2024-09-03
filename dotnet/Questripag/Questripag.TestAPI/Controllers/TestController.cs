using Microsoft.AspNetCore.Mvc;

namespace Questripag.TestAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TestController : ControllerBase
    {
        [HttpGet]
        public int Get(Query<IQueryOptions> query, [FromQuery] int manualQueryParam)
        {
            throw new NotImplementedException();
        }
    }

    public interface IQueryOptions
    {
        public int Prop1 { get; set; }
        public DateOnly Prop2 { get; set; }
        public LeftOrRight Prop3 { get; set; }
    }

    public enum LeftOrRight { Left, Right };
}
