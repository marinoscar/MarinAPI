using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BCCR.TipoDeCambio
{
    public record IndicatorRecord(int Code, int TypeId, string Type, string? Name, double Value, DateTime Date);
}
