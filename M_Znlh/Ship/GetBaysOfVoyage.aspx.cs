//
//文件名：    GetBaysOfVoyage.aspx.cs
//功能描述：  获取某航次所有贝位号
//创建时间：  2016/11/25
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

namespace M_Znlh.Ship
{
    public partial class GetBaysOfVoyage : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            //身份校验
            if (!InterfaceTool.IdentityVerify(Request))
            {
                Json = JsonConvert.SerializeObject(new DicPackage(false, null, "身份认证错误！").DicInfo());
                return;
            }

            //航次编码
            string strShip_Id = Request.Params["Ship_Id"];
            //strShip_Id = "600";


            try
            {
                if (strShip_Id == null)
                {
                    Json = JsonConvert.SerializeObject(new DicPackage(false, null, "参数错误，获取某航次所有贝位号数据失败！").DicInfo());
                    return;
                }

                string strSql =
                    string.Format(@"select t.SHIP_ID,t.V_ID,t.ENG_VESSEL,t.CHI_VESSEL,t.LOCATION,t.BAY_NUM,t.BAY_COL,t.BAY_ROW,t.SBAYNO,t.TBAYNO,t.JBAYNO,t.USER_CHAR,t.SCREEN_ROW,t.SCREEN_COL,t.JOINT,t.CODE_LOAD_PORT,t.CODE_UNLOAD_PORT,t.CODE_DELIVERY,t.MOVED,t.UNLOAD_MARK,t.WORK_NO,t.DANGER_GRADE,t.DEGREE_SETTING,t.DEGREE_UNIT,t.MIN_DEGREE,t.MAX_DEGREE,t.BAYNO,t.OLDBAYNO,t.CODE_CRANE,t.IMAGE_ID,t.BAYNUM,t.BAYCOL,t.BAYROW,t.CONTAINER_NO,t.SIZE_CON,t.CONTAINER_TYPE,t.CODE_EMPTY,t.WEIGHT,t.WORK_DATE,t.SEALNO,t.MOVED_NAME,t.INOUTMARK,t.TransMark,t.JJR,t.YB,t.NAME 
                                    from vcon_bay_detail t  
                                    where t.SHIP_ID='{0}'",
                                    strShip_Id);
                var dt = new Leo.Oracle.DataAccess(RegistryKey.KeyPathTally).ExecuteTable(strSql);
                if (dt.Rows.Count <= 0)
                {
                    Json = JsonConvert.SerializeObject(new DicPackage(true, null, "暂无数据！").DicInfo());
                    return;
                }

                string[ ] strArray = new string[dt.Rows.Count];
                for (int iRow = 0; iRow < dt.Rows.Count; iRow++)
                {
                    strArray[iRow] = dt.Rows[iRow]["baynum"].ToString();
                }

                Json = JsonConvert.SerializeObject(new DicPackage(true, strArray, null).DicInfo());
            }
            catch (Exception ex)
            {
                Json = JsonConvert.SerializeObject(new DicPackage(false, null, string.Format("{0}：获取某航次所有贝位号数据发生异常。{1}", ex.Source, ex.Message)).DicInfo());
            }
        }
        protected string Json;
    }
}