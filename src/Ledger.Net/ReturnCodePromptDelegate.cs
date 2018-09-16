using System.Threading.Tasks;

namespace Ledger.Net
{
    public delegate Task ReturnCodePromptDelegate(int returnCode, string member);
}
