using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MasterDetails.Models;

namespace MasterDetails.Controllers
{
    public class HomeController : Controller
    {

        private ordersEntities db = new ordersEntities();
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult getOrders()
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var model = (db.OrderMasters.ToList()
                        .Select(x => new
                        {
                            masterId = x.MasterId,
                            customerName = x.CustomerName,
                            address = x.Address,
                            orderDate = x.OrderDate.ToString("D")
                        })).ToList();

            return Json(new
            {
                draw = draw,
                recordsFiltered = model.Count,
                recordsTotal = model.Count,
                data = model
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult saveOrder(OrderViewModel order)
        {
            var masterId = Guid.NewGuid();
            var orderMaster = new OrderMaster()
            {
                MasterId = masterId,
                CustomerName = order.CustomerName,
                Address = order.Address,
                OrderDate = DateTime.Now
            };
            db.OrderMasters.Add(orderMaster);
            //Process Order details

            if (order.OrderDetails.Any())
            {
                foreach (var item in order.OrderDetails)
                {
                    var detailId = Guid.NewGuid();
                    var orderDetails = new OrderDetail()
                    {
                        DetailId= detailId,
                        MasterId =masterId,
                        Amount =decimal.Parse(item.Amount),
                        ProductName = item.ProductName,
                        Quantity =int.Parse(item.Quantity)
                    };

                    db.OrderDetails.Add(orderDetails);
                    
                }
            }

            try
            {
                if (db.SaveChanges() > 0)
                {
                    return Json(new { error = false, message = "Order saved successfully" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { error = true, message = ex.Message });
            }

            return Json(new { error = true, message = "An unknown error has occured" });
        }
        public ActionResult getSingleOrder(Guid orderId)
        {
            var model = (from ord in db.OrderMasters
                         where ord.MasterId == orderId
                         select new OrderViewModel()
                         {
                             MasterId = ord.MasterId,
                             CustomerName = ord.CustomerName,
                             Address = ord.Address
                         }).SingleOrDefault();

            if (model != null)
            {
                model.OrderDetails = (from od in db.OrderDetails
                                      where od.MasterId == model.MasterId
                                      select new OrderDetailsViewModel()
                                      {
                                          DetailId = od.DetailId,
                                          Amount = od.Amount.ToString(),
                                          ProductName = od.ProductName,
                                          Quantity = od.Quantity.ToString()
                                      }).ToList();
            }

            return Json(new { result = model }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult deleteOrderItem(Guid id)
        {
            var order = db.OrderDetails.Find(id);
            if (null != order)
            {
                db.OrderDetails.Remove(order);
                db.SaveChanges();
                return Json(new { error = false });
            }
            return Json(new { error = true });
        }
        public ActionResult getSingleOrderDetail(Guid id)
        {
            var orderDetail = (from od in db.OrderDetails
                               where od.DetailId == id
                               select new OrderDetailsViewModel()
                               {
                                   DetailId = od.DetailId,
                                   Amount = od.Amount.ToString(),
                                   ProductName = od.ProductName,
                                   Quantity = od.Quantity.ToString()
                               }).SingleOrDefault();

            return Json(new { result = orderDetail }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult updateOrder(Guid orderId)
        {
            return null;
        }
    }
}