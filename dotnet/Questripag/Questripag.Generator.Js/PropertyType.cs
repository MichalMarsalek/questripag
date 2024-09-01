using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Questripag.Generator.Js;

public enum PropertyType
{
    Other,
    Boolean,
    Integer,
    Double,
    Decimal,
    String,
    Enumerable,
    Guid,
    DateTime,
    DateOnly,
    TimeOnly,
    DateTimeOffset
}
