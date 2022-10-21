using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading;

using Legacy;

namespace TradeBotyoupin898
{
    internal class Main
    {
        private const int kapi_call_interval = 5000;
        private const int kapi_fetch_interval = 600000;

        private YouPinAPI youpinAPI;
        private YouPinAPILegacy youpinAPILegacy;

        private SteamAPI steamAPI;
        private SteamAPILegacy steamAPILegacy;

        public Main()
        {
            Stream stream = Console.OpenStandardInput();
            Console.SetIn(new StreamReader(stream, Encoding.Default, false, 5000));
            steamAPI = new SteamAPI();
            youpinAPI = new YouPinAPI();
            youpinAPILegacy = new YouPinAPILegacy();
            steamAPILegacy = new SteamAPILegacy();
        }

        public void Start()
        {
            while (true)
            {
                ushort apiCallcount = 0;
                processQuery();
                Thread.Sleep(kapi_call_interval);
                processQueryLegacy();

                Thread.Sleep(api_fetch_interval(apiCallcount));
            }
        }

        private void processQuery()
        {
            var todoList = youpinAPI.GetToDoList();
            if (todoList == null || todoList.Count == 0)
            {
                Console.WriteLine($"{DateTime.UtcNow}\t{nameof(processQuery)}\t当前没有报价");
                Console.WriteLine();
                return;
            }

            try
            {
                toDoListHandle(todoList);
            }
            catch (InvalidEnumArgumentException NotHandleException)
            {
                Console.WriteLine(NotHandleException);
            }
        }

        private void processQueryLegacy()
        {
            var todoList = youpinAPILegacy.GetToDoList();
            if (todoList == null || todoList.Count == 0)
            {
                Console.WriteLine($"{DateTime.UtcNow}\t{nameof(processQueryLegacy)}\t当前没有报价");
                Console.WriteLine();
                return;
            }

            try
            {
                toDoListHandleLegacy(todoList);
            }
            catch (InvalidEnumArgumentException NotHandleException)
            {
                Console.WriteLine(NotHandleException);
            }
        }

        private void toDoListHandle(List<ToDo.TodoDataItem> todoList)
        {
            foreach (var todo in todoList)
            {
                string orderID = todo.OrderNo;
                OrderData order = youpinAPI.GetOrder(orderID);

                if (order == null)
                    continue;

                BusinessType businessType;

                businessType = (BusinessType)order.TradeType.Type;

                /**
                switch (businessType)
                {
                    case BusinessType.Lease:
                        leaseHandle(order);
                        break;

                    case BusinessType.Sell:
                        sellHandle(order);
                        break;

                    default:
                        throw new InvalidEnumArgumentException("尚未支持的业务类型", order.BusinessType, typeof(BusinessType));
                }
                **/
                // 由于对于其他订单状态暂时未知，默认由sellHandle处理
                sellHandle(order);
            }
        }

        private void toDoListHandleLegacy(List<ToDoLegacy.TodoDataItemLegacy> todoList)
        {
            foreach (var todo in todoList)
            {
                string orderID = todo.OrderNo;
                OrderDataLegacy order = youpinAPILegacy.GetOrder(orderID);
                BusinessTypeLegacy businessType;

                businessType = (BusinessTypeLegacy)order.BusinessType;

                switch (businessType)
                {
                    case BusinessTypeLegacy.Lease:
                        leaseHandleLegacy(order);
                        break;

                    case BusinessTypeLegacy.Sell:
                        sellHandleLegacy(order);
                        break;

                    default:
                        throw new InvalidEnumArgumentException("尚未支持的业务类型", order.BusinessType, typeof(BusinessType));
                }
            }
        }

        private void leaseHandleLegacy(OrderDataLegacy order)
        {
            LeaseStatus leaseStatus = (LeaseStatus)order.LeaseStatus;

            bool needPhoneConfirm;

            switch (leaseStatus)
            {
                case LeaseStatus.Paied:
                    needPhoneConfirm = true;
                    break;

                case LeaseStatus.Remand:
                    // 获取归还订单单号，代办所给单号为租赁用
                    order = youpinAPILegacy.GetLeaseReturnOrder(order.OrderNo);
                    // 归还订单不需要手机确认
                    needPhoneConfirm = false;
                    break;

                default:
                    throw new InvalidEnumArgumentException("尚未支持的租赁订单状态", order.LeaseStatus, typeof(LeaseStatus));
            }

            Console.WriteLine(order.CommodityName);
            Console.WriteLine(order.SteamOfferId, order.OtherSteamId);

            steamConfrimLegacy(order, needPhoneConfirm);
        }


        /// <summary>
        /// 出售订单没有多余的状态
        /// </summary>
        /// <param name="order"></param>
        private void sellHandle(OrderData order)
        {
            steamConfrim(order);
        }

        private void sellHandleLegacy(OrderDataLegacy order)
        {
            steamConfrimLegacy(order);
        }

        private void steamConfrim(OrderData order, bool needPhoneConfirm = true)
        {
            steamAPI.AcceptOffer(order);

            if (needPhoneConfirm)
            {
                var confs = steamAPI.GetConfirmation();
                foreach (var conf in confs)
                {
                    if (conf.Creator != order.TradeOfferId) break;
                    while (steamAPI.AcceptConfirmation(conf)) ;
                }
            }
        }

        private void steamConfrimLegacy(OrderDataLegacy order, bool needPhoneConfirm = true)
        {
            steamAPILegacy.AcceptOffer(order);

            if (needPhoneConfirm)
            {
                var confs = steamAPILegacy.GetConfirmation();
                foreach (var conf in confs)
                {
                    if (conf.Creator != ulong.Parse(order.SteamOfferId)) break;
                    while (steamAPI.AcceptConfirmation(conf)) ;
                }
            }
        }

        /// <summary>
        /// Substract already wait time from total interval.
        /// </summary>
        /// <param name="callCount">Total call times in this fetch.</param>
        /// <returns>Remain time to wait.</returns>
        private int api_fetch_interval(ushort callCount)
        {
            int remainTime = kapi_fetch_interval - (callCount * kapi_call_interval);
            if (remainTime <= kapi_call_interval)
            {
#if DEBUG
                Console.WriteLine($"{DateTime.UtcNow}\t过多的API调用:{callCount}次!");
#endif
                return kapi_call_interval;
            }
            else
            {
                return remainTime;
            }
        }
    }
}
