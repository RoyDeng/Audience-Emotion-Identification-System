using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace AEIS
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            //首頁
            routes.MapRoute(
                name: "Index", //路由名稱
                url: "Index/{action}/{id}",   //URL及參數
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );

            //隱私權與服務條款
            routes.MapRoute(
                name: "Terms",
                url: "Terms/{action}/{id}",
                defaults: new { controller = "Home", action = "Terms", id = UrlParameter.Optional }
            );

            //會員登入
            routes.MapRoute(
                name: "Login",
                url: "Login/{action}/{id}",
                defaults: new { controller = "User", action = "Login", id = UrlParameter.Optional }
            );

            //操作登入
            routes.MapRoute(
                name: "LoginProcess",
                url: "LoginProcess/{action}/{id}",
                defaults: new { controller = "User", action = "LoginProcess", id = UrlParameter.Optional }
            );

            //會員註冊
            routes.MapRoute(
                name: "Register",
                url: "Register/{action}/{id}",
                defaults: new { controller = "User", action = "Register", id = UrlParameter.Optional }
            );

            //操作註冊
            routes.MapRoute(
                name: "RegisterProcess",
                url: "RegisterProcess/{action}/{id}",
                defaults: new { controller = "User", action = "RegisterProcess", id = UrlParameter.Optional }
            );

            //忘記密碼
            routes.MapRoute(
                name: "ForgetPassword",
                url: "ForgetPassword/{action}/{id}",
                defaults: new { controller = "User", action = "ForgetPassword", id = UrlParameter.Optional }
            );

            //操作忘記密碼
            routes.MapRoute(
                name: "ForgetPasswordProcess",
                url: "ForgetPasswordProcess/{action}/{id}",
                defaults: new { controller = "User", action = "ForgetPasswordProcess", id = UrlParameter.Optional }
            );

            //操作更新密碼
            routes.MapRoute(
                name: "ChangePasswordProcess",
                url: "ChangePasswordProcess/{action}/{id}",
                defaults: new { controller = "User", action = "ChangePasswordProcess", id = UrlParameter.Optional }
            );

            //會員登出
            routes.MapRoute(
                name: "Logout",
                url: "Logout/{action}/{id}",
                defaults: new { controller = "User", action = "Logout", id = UrlParameter.Optional }
            );

            //檢查使用者名稱是否重複
            routes.MapRoute(
                name: "CheckUsernameDuplicate",
                url: "CheckUsernameDuplicate/{action}/{id}",
                defaults: new { controller = "Tool", action = "CheckUsernameDuplicate", id = UrlParameter.Optional }
            );

            //我的商品
            routes.MapRoute(
                name: "MyProduct",
                url: "MyProduct/{action}/{id}",
                defaults: new { controller = "User", action = "MyProduct", id = UrlParameter.Optional }
            );

            //我的商品明細
            routes.MapRoute(
                name: "MyProductDetail",
                url: "MyProductDetail/{action}/{id}",
                defaults: new { controller = "User", action = "MyProductDetail", id = UrlParameter.Optional }
            );

            //訂單查詢
            routes.MapRoute(
                name: "MyOrder",
                url: "MyOrder/{action}/{id}",
                defaults: new { controller = "User", action = "MyOrder", id = UrlParameter.Optional }
            );

            //訂單明細
            routes.MapRoute(
                name: "OrderDetail",
                url: "OrderDetail/{action}/{id}",
                defaults: new { controller = "Order", action = "OrderDetail", id = UrlParameter.Optional }
            );

            //會員設定
            routes.MapRoute(
                name: "Profile",
                url: "Profile/{action}/{id}",
                defaults: new { controller = "User", action = "Profile", id = UrlParameter.Optional }
            );

            //操作會員資料
            routes.MapRoute(
                name: "UpdateUserProcess",
                url: "UpdateUserProcess/{action}/{id}",
                defaults: new { controller = "User", action = "UpdateUserProcess", id = UrlParameter.Optional }
            );

            //操作商店資料
            routes.MapRoute(
                name: "ModifyShopProcess",
                url: "ModifyShopProcess/{action}/{id}",
                defaults: new { controller = "Shop", action = "ModifyShopProcess", id = UrlParameter.Optional }
            );

            //商店頁面
            routes.MapRoute(
                name: "Shops",
                url: "Shops/{action}/{id}",
                defaults: new { controller = "Shop", action = "Index", id = UrlParameter.Optional }
            );

            //商店明細
            routes.MapRoute(
                name: "ShopDetail",
                url: "ShopDetail/{action}/{id}",
                defaults: new { controller = "Shop", action = "ShopDetail", id = UrlParameter.Optional }
            );

            //商品頁面
            routes.MapRoute(
                name: "Products",
                url: "Products/{action}/{id}",
                defaults: new { controller = "Product", action = "Index", id = UrlParameter.Optional }
            );

            //商品明細
            routes.MapRoute(
                name: "ProductDetail",
                url: "ProductDetail/{action}/{id}",
                defaults: new { controller = "Product", action = "ProductDetail", id = UrlParameter.Optional }
            );

            //新增商品
            routes.MapRoute(
                name: "CreateProduct",
                url: "CreateProduct/{action}/{id}",
                defaults: new { controller = "Product", action = "CreateProduct", id = UrlParameter.Optional }
            );

            //操作商品資料
            routes.MapRoute(
                name: "ModifyProductProcess",
                url: "ModifyProductProcess/{action}/{id}",
                defaults: new { controller = "Product", action = "ModifyProductProcess", id = UrlParameter.Optional }
            );

            //影片頁面
            routes.MapRoute(
                name: "Videos",
                url: "Videos/{action}/{id}",
                defaults: new { controller = "Video", action = "Index", id = UrlParameter.Optional }
            );

            //新增直播
            routes.MapRoute(
                name: "CreateVideo",
                url: "CreateVideo/{action}/{id}",
                defaults: new { controller = "Video", action = "CreateVideo", id = UrlParameter.Optional }
            );

            //新增直播過程
            routes.MapRoute(
                name: "StreamVideoProcess",
                url: "StreamVideoProcess/{action}/{id}",
                defaults: new { controller = "Video", action = "StreamVideoProcess", id = UrlParameter.Optional }
            );

            //播送頁面
            routes.MapRoute(
                name: "StreamVideo",
                url: "StreamVideo/{action}/{id}",
                defaults: new { controller = "Video", action = "StreamVideo", id = UrlParameter.Optional }
            );

            //結束直播過程
            routes.MapRoute(
                name: "UploadVideoProcess",
                url: "UploadVideoProcess/{action}/{id}",
                defaults: new { controller = "Video", action = "UploadVideoProcess", id = UrlParameter.Optional }
            );

            //上傳影片
            routes.MapRoute(
                name: "UploadVideo",
                url: "UploadVideo/{action}/{id}",
                defaults: new { controller = "Video", action = "UploadVideo", id = UrlParameter.Optional }
            );

            //觀看頁面
            routes.MapRoute(
                name: "WatchVideo",
                url: "WatchVideo/{action}/{id}",
                defaults: new { controller = "Video", action = "WatchVideo", id = UrlParameter.Optional }
            );

            //上傳表情數據過程
            routes.MapRoute(
                name: "UploadEmotionProcess",
                url: "UploadEmotionProcess/{action}/{id}",
                defaults: new { controller = "Video", action = "UploadEmotionProcess", id = UrlParameter.Optional }
            );

            //購物車
            routes.MapRoute(
                name: "Cart",
                url: "Cart/{action}/{id}",
                defaults: new { controller = "Cart", action = "Index", id = UrlParameter.Optional }
            );

            //加入購物車
            routes.MapRoute(
                name: "AddToCart",
                url: "AddToCart/{action}/{id}",
                defaults: new { controller = "Cart", action = "AddToCart", id = UrlParameter.Optional }
            );

            //移除購物車項目
            routes.MapRoute(
                name: "RemoveCartItem",
                url: "RemoveCartItem/{action}/{id}",
                defaults: new { controller = "Cart", action = "RemoveCartItem", id = UrlParameter.Optional }
            );

            //更新購物車
            routes.MapRoute(
                name: "UpdateCart",
                url: "UpdateCart/{action}/{id}",
                defaults: new { controller = "Cart", action = "UpdateCart", id = UrlParameter.Optional }
            );

            //清空購物車
            routes.MapRoute(
                name: "CleanCart",
                url: "CleanCart/{action}/{id}",
                defaults: new { controller = "Cart", action = "CleanCart", id = UrlParameter.Optional }
            );

            //結帳頁面
            routes.MapRoute(
                name: "CheckOut",
                url: "CheckOut/{action}/{id}",
                defaults: new { controller = "Order", action = "CheckOut", id = UrlParameter.Optional }
            );

            //操作結帳過程
            routes.MapRoute(
                name: "CheckOutProcess",
                url: "CheckOutProcess/{action}/{id}",
                defaults: new { controller = "Order", action = "CheckOutProcess", id = UrlParameter.Optional }
            );

            //完成結帳頁面
            routes.MapRoute(
                name: "FinishCheckOut",
                url: "FinishCheckOut/{action}/{id}",
                defaults: new { controller = "Order", action = "FinishCheckOut", id = UrlParameter.Optional }
            );
        }
    }
}
