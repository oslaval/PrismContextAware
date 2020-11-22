using System;
using System.Threading.Tasks;

namespace AsyncVoid
{
    public interface IErrorHandler
    {
        void HandleError(object sender, Exception e);
    }
}
