﻿//
//文件名：    DoubleLift.aspx.cs
//功能描述：  双吊
//创建时间：  2017/6/24
//作者：      
//修改时间：  
//修改描述：  暂无
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Newtonsoft.Json;
using Leo;
using ServiceInterface.Common;
using Leo.Oracle;

namespace M_Znlh.Move
{
    public partial class DoubleLift : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            //身份校验
            if (!InterfaceTool.IdentityVerify(Request))
            {
                Json = JsonConvert.SerializeObject(new DicPackage(false, null, "身份认证错误！").DicInfo());
                return;
            }

            //航次Id
            string strShipId = Request.Params["ShipId"];
            //船舶Id
            string strV_Id = Request.Params["V_Id"];
            //箱号1
            string strContainerNo1 = Request.Params["ContainerNo1"];
            //箱号2
            string strContainerNo2 = Request.Params["ContainerNo2"];
            //目标贝号
            string strEndBayno = Request.Params["EndBayno"];
            //用户ID
            string strUserId = Request.Params["UserId"];

            //strShipId = "195";
            //strV_Id = "6321";
            ////strContainerNo1 = "FCIU4708678";
            //strContainerNo1 = "ZGXU2382004";
            //strContainerNo2 = "CXDU2375239";
            //strEndBayno = "090484";
            //strUserId = "215";


            AppLog log = new AppLog(Request);
            log.strBehavior = "双吊";
            log.strBehaviorURL = "/Move/DoubleLift.aspx";
            log.strAccount = strUserId;

            try
            {
                if (strShipId == null || strV_Id == null || strContainerNo1 == null || strContainerNo2 == null || strEndBayno == null || strUserId == null)
                {
                    Json = JsonConvert.SerializeObject(new DicPackage(false, null, "参数错误，双吊失败！").DicInfo());
                    return;
                }

                da.BeginTransaction();

                IMoveBay iMoveBay = new IMoveBay(log);
                iMoveBay.setDataAccess(da);

                //校验目标贝位号是否存在
                if (!iMoveBay.isExistBayno(strV_Id, strEndBayno))
                {

                    Json = JsonConvert.SerializeObject(new DicPackage(true, null, "错误贝位号").DicInfo());
                    log.LogCatalogFailure(string.Format("错误贝位号"));
                    return;
                }

                //后通目标贝位号
                String backEndBayno = Convert.ToString((Convert.ToInt16(strEndBayno.Substring(0, 2)) + 2)).PadLeft(2, '0') + strEndBayno.Substring(2).PadLeft(4, '0');

                //校验后通目标贝位号是否存在
                if (!iMoveBay.isExistBayno(strV_Id, backEndBayno))
                {

                    Json = JsonConvert.SerializeObject(new DicPackage(true, null, Convert.ToString(strEndBayno.Substring(0, 2)) + "贝不通贝").DicInfo());
                    log.LogCatalogFailure(string.Format(Convert.ToString(strEndBayno.Substring(0, 2)) + "贝不通贝"));
                    return;
                }

                string strOperareName = string.Empty;
                string strSql = string.Format(@"select distinct name
                                                from SYSTEM_USER_TABLE 
                                                where user_id='{0}'",
                                                strUserId);
                var dt = new Leo.SqlServer.DataAccess(RegistryKey.KeyPathTallySqlServer).ExecuteTable(strSql);
                if (dt.Rows.Count > 0)
                {

                    strOperareName = Convert.ToString(dt.Rows[0]["name"]);
                }


                //获取待移集装箱信息
                strSql =
                    string.Format(@"select size_con,bayno
                                    from con_image 
                                    where container_no='{0}' and ship_id={1}",
                                    strContainerNo1, strShipId);
                var dt1 = da.ExecuteTable(strSql);
                if (dt1.Rows.Count <= 0)
                {

                    Json = JsonConvert.SerializeObject(new DicPackage(true, null, "待调集装箱号不存在").DicInfo());
                    log.LogCatalogFailure(string.Format("待调集装箱号不存在"));
                    return;
                }

                strSql =
                     string.Format(@"select size_con,bayno
                                     from con_image 
                                     where container_no='{0}' and ship_id={1}",
                                     strContainerNo2, strShipId);
                var dt2 = da.ExecuteTable(strSql);
                if (dt2.Rows.Count <= 0)
                {

                    Json = JsonConvert.SerializeObject(new DicPackage(true, null, "待调集装箱号不存在").DicInfo());
                    log.LogCatalogFailure(string.Format("待调集装箱号不存在"));
                    return;
                }

                Json = iMoveBay.DoubleLift(strShipId, strContainerNo1, Convert.ToString(dt1.Rows[0]["bayno"]), strContainerNo2, Convert.ToString(dt2.Rows[0]["bayno"]), strEndBayno, strOperareName);

                if (Json != null)
                {
                    return;
                }

                da.CommitTransaction();

                Json = JsonConvert.SerializeObject(new DicPackage(true, null, "上传成功！").DicInfo());
                log.LogCatalogSuccess();
            }
            catch (Exception ex)
            {
                da.RollbackTransaction();
                Json = JsonConvert.SerializeObject(new DicPackage(false, null, string.Format("{0}：双吊发生异常。{1}", ex.Source, ex.Message)).DicInfo());
                log.LogCatalogFailure(string.Format("{0}：双吊发生异常。{1}", ex.Source, ex.Message));
            }
        }

        protected string Json;
        DataAccess da = (DataAccess)new Leo.Oracle.DataAccess(RegistryKey.KeyPathTally);


    }
}
