using DynamicData;
using Loqui;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Core;
using Mutagen.Bethesda.WPF;
using Newtonsoft.Json.Linq;
using Noggog;
using Noggog.WPF;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Serilog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text.Json;
using System.Windows.Input;

namespace Synthesis.Bethesda.GUI
{
    public class EnumerableFormLinkSettingsVM : SettingsNodeVM
    {
        public ObservableCollection<FormKeyItemViewModel> Values { get; } = new ObservableCollection<FormKeyItemViewModel>();

        private FormKey[] _defaultVal;
        private readonly IObservable<ILinkCache> _linkCacheObs;
        private readonly ObservableAsPropertyHelper<ILinkCache?> _LinkCache;
        private readonly string _typeName;
        public ILinkCache? LinkCache => _LinkCache.Value;

        public IEnumerable<Type> ScopedTypes { get; private set; } = Enumerable.Empty<Type>();

        public EnumerableFormLinkSettingsVM(
            IObservable<ILinkCache> linkCache,
            string memberName,
            string typeName,
            IEnumerable<FormKey> defaultVal)
            : base(memberName)
        {
            _defaultVal = defaultVal.ToArray();
            _linkCacheObs = linkCache;
            _typeName = typeName;
            _LinkCache = linkCache
                .ToGuiProperty(this, nameof(LinkCache), default(ILinkCache?));
        }

        public static SettingsNodeVM Factory(SettingsParameters param, string memberName, string typeName, object? defaultVal)
        {
            var defaultKeys = new List<FormKey>();
            if (defaultVal is IEnumerable e)
            {
                foreach (var item in e)
                {
                    if (item is IFormLink link)
                    {
                        defaultKeys.Add(link.FormKey);
                    }
                }
            }
            return new EnumerableFormLinkSettingsVM(
                param.LinkCache,
                memberName: memberName,
                typeName: typeName,
                defaultKeys);
        }

        public override void Import(JsonElement property, ILogger logger)
        {
            Values.Clear();
            foreach (var elem in property.EnumerateArray())
            {
                if (FormKey.TryFactory(elem.GetString(), out var formKey))
                {
                    Values.Add(new FormKeyItemViewModel(formKey));
                }
                else
                {
                    Values.Add(new FormKeyItemViewModel(FormKey.Null));
                }
            }
        }

        public override void Persist(JObject obj, ILogger logger)
        {
            obj[MemberName] = new JArray(Values
                .Select(x =>
                {
                    if (x.FormKey.IsNull)
                    {
                        return string.Empty;
                    }
                    else
                    {
                        return x.FormKey.ToString();
                    }
                }).ToArray());
        }

        public override SettingsNodeVM Duplicate()
        {
            return new EnumerableFormLinkSettingsVM(_linkCacheObs, _typeName, string.Empty, _defaultVal);
        }

        public override void WrapUp()
        {
            _defaultVal = _defaultVal.Select(x => FormKeySettingsVM.StripOrigin(x)).ToArray();
            Values.SetTo(_defaultVal.Select(x =>
            {
                return new FormKeyItemViewModel(x);
            }));

            if (LoquiRegistration.TryGetRegisterByFullName(_typeName, out var regis))
            {
                ScopedTypes = regis.GetterType.AsEnumerable();
            }
            else if (LinkInterfaceMapping.TryGetByFullName(_typeName, out var interfType))
            {
                ScopedTypes = interfType.AsEnumerable();
            }
            else
            {
                throw new ArgumentException($"Can't create a formlink control for type: {_typeName}");
            }
        }
    }
}