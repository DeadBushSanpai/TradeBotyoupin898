using System.Collections.Generic;

namespace Legacy
{
    public class ToDoLegacy
    {
        /// <summary>
        /// 
        /// </summary>
        public int Code { get; set; }
        /// <summary>
        /// 请求成功
        /// </summary>
        public string Msg { get; set; }
        ///<summary>
        /// 
        /// </summary>
        public List<TodoDataItemLegacy> Data { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public class TodoDataItemLegacy
        {
            public int Type { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string OrderNo { get; set; }
            /// <summary>
            /// 有承租方下单，等待确认报价
            /// </summary>
            public string Message { get; set; }

            public string CommodityName { get; set; }
        }


    }
}

