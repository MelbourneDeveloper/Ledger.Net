using System;
using System.Threading.Tasks;

namespace Ledger.Net
{
    public delegate Task ErrorPromptDelegate(int? returnCode, Exception exception, string member);
}
