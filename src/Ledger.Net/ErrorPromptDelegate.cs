using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Ledger.Net
{
    public delegate Task ErrorPromptDelegate(int? returnCode, Exception exception, [CallerMemberName] string member = null);
}
