using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzMigrate.Model
{
    public class SingleKeyEntity
    {
        [JsonProperty("id")]
#pragma warning disable IDE1006 // Naming Styles
        protected string id { get; set; }
#pragma warning restore IDE1006 // Naming Styles

        public virtual string Id
        {
            get
            {
                return id;
            }
            set
            {
                id = value;
            }
        }
    }
}
