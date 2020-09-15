using System.Collections.Generic;
using Project.Domain.SeedWork;

namespace Project.Domain.AggregatesModel {
    public class ProjectProperty : ValueObject {
        public ProjectProperty () {

        }
        public ProjectProperty (string key, string text, string value) {
            Key = key;
            Text = text;
            Value = value;
        }

        public string Key { get; set; }
        public string Text { get; set; }
        public string Value { get; set; }
        protected override IEnumerable<object> GetAtomicValues () {
            yield return Key;
            yield return Text;
            yield return Value;
        }
    }
}