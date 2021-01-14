using Newtonsoft.Json.Linq;
using ReactiveUI.Fody.Helpers;
using Serilog;
using System.Text.Json;

namespace Synthesis.Bethesda.GUI
{
    public interface IBasicSettingsNodeVM
    {
        object Value { get; }
    }

    public abstract class BasicSettingsNodeVM<T> : SettingsNodeVM, IBasicSettingsNodeVM
    {
        [Reactive]
        public T Value { get; set; }

        object IBasicSettingsNodeVM.Value => this.Value!;

        public BasicSettingsNodeVM(string memberName, object? defaultVal)
            : base(memberName)
        {
            if (defaultVal is T item)
            {
                Value = item;
            }
            else
            {
                Value = default!;
            }
        }

        public override void Import(JsonElement property, ILogger logger)
        {
            Value = Get(property);
        }

        public override void Persist(JObject obj, ILogger logger)
        {
            obj[MemberName] = Value as JToken;
        }

        public abstract T Get(JsonElement property);

        public abstract T GetDefault();
    }
}