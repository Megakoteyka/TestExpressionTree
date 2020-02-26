using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParamProvider
{
    public interface IMethod
    {

    }

    public interface IParam
    {
        string Name { get; }
        string Description { get; }
    }

    public interface IParamProvider
    {
        IReadOnlyCollection<IParam> Params { get; }
    }
}
