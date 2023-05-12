using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace MyWeb.Pages.Admin
{
    public interface INavLink
    {
        public RenderFragment ChildContent { get; }
    }
    public interface ICancelable
    {
        public void Cancel();
    }
    public interface IExamine
    {
        public Task Examine(int action);
    }
    public enum ExamineType
    {
        通过,
        拒绝,
        删除,
        封禁,
    }
}
