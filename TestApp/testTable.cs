using System;

namespace Entity
{

    public class TestTableEntity
    {
        public int ID { get; set; }
        public Nullable<DateTime> InsertDate { get; set; }
        public Nullable<DateTime> UpdateDate { get; set; }
        public string? UpdateUser { get; set; }
        public int? Count { get; set; }


    }
}
