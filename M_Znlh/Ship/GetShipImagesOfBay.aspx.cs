//
//文件名：    GetShipImagesOfBay.aspx.cs
//功能描述：  获取某贝贝船图数据
//创建时间：  2017/1/17
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
    public partial class GetShipImagesOfBay : System.Web.UI.Page
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
            //贝号
            string strBay_Num = Request.Params["Bay_Num"];
            //strShip_Id = "600";
            //strBay_Num = "01";


            try
            {
                if (strShip_Id == null)
                {
                    Json = JsonConvert.SerializeObject(new DicPackage(false, null, "参数错误，获取某航次所有贝船图数据失败！").DicInfo());
                    return;
                }

                string strSql =
                    string.Format(@"select t.SHIP_ID,t.V_ID,t.ENG_VESSEL,t.CHI_VESSEL,t.LOCATION,t.BAY_NUM,t.BAY_COL,t.BAY_ROW,t.SBAYNO,t.TBAYNO,t.JBAYNO,t.USER_CHAR,t.SCREEN_ROW,t.SCREEN_COL,t.JOINT,t.CODE_LOAD_PORT,t.CODE_UNLOAD_PORT,t.CODE_DELIVERY,t.MOVED,t.UNLOAD_MARK,t.WORK_NO,t.DANGER_GRADE,t.DEGREE_SETTING,t.DEGREE_UNIT,t.MIN_DEGREE,t.MAX_DEGREE,t.BAYNO,t.OLDBAYNO,t.CODE_CRANE,t.IMAGE_ID,t.BAYNUM,t.BAYCOL,t.BAYROW,t.CONTAINER_NO,t.SIZE_CON,t.CONTAINER_TYPE,t.CODE_EMPTY,t.WEIGHT,t.WORK_DATE,t.SEALNO,t.MOVED_NAME,t.INOUTMARK,t.TransMark,t.JJR,t.YB,t.NAME 
                                    from vcon_bay_detail t  
                                    where t.SHIP_ID='{0}' and t.BAY_NUM='{1}'", 
                                    strShip_Id, strBay_Num);
                var dt = new Leo.Oracle.DataAccess(RegistryKey.KeyPathTally).ExecuteTable(strSql);
                if (dt.Rows.Count <= 0)
                {
                    Json = JsonConvert.SerializeObject(new DicPackage(true, null, "暂无数据！").DicInfo());
                    return;
                }

                string[,] strArray = new string[dt.Rows.Count, 46];
                for (int iRow = 0; iRow < dt.Rows.Count; iRow++)
                {
                    strArray[iRow, 0] = dt.Rows[iRow]["SHIP_ID"].ToString();
                    strArray[iRow, 1] = dt.Rows[iRow]["V_ID"].ToString();
                    strArray[iRow, 2] = dt.Rows[iRow]["ENG_VESSEL"].ToString();
                    strArray[iRow, 3] = dt.Rows[iRow]["CHI_VESSEL"].ToString();
                    strArray[iRow, 4] = dt.Rows[iRow]["LOCATION"].ToString();
                    strArray[iRow, 5] = dt.Rows[iRow]["BAY_NUM"].ToString();
                    strArray[iRow, 6] = dt.Rows[iRow]["BAY_COL"].ToString();
                    strArray[iRow, 7] = dt.Rows[iRow]["BAY_ROW"].ToString();
                    strArray[iRow, 8] = dt.Rows[iRow]["SBAYNO"].ToString();
                    strArray[iRow, 9] = dt.Rows[iRow]["TBAYNO"].ToString();
                    strArray[iRow, 10] = dt.Rows[iRow]["JBAYNO"].ToString();
                    strArray[iRow, 11] = dt.Rows[iRow]["USER_CHAR"].ToString();
                    strArray[iRow, 12] = dt.Rows[iRow]["SCREEN_ROW"].ToString();
                    strArray[iRow, 13] = dt.Rows[iRow]["SCREEN_COL"].ToString();
                    strArray[iRow, 14] = dt.Rows[iRow]["JOINT"].ToString();
                    strArray[iRow, 15] = dt.Rows[iRow]["CODE_LOAD_PORT"].ToString();
                    strArray[iRow, 16] = dt.Rows[iRow]["CODE_UNLOAD_PORT"].ToString();
                    strArray[iRow, 17] = dt.Rows[iRow]["CODE_DELIVERY"].ToString();
                    strArray[iRow, 18] = dt.Rows[iRow]["MOVED"].ToString();
                    strArray[iRow, 19] = dt.Rows[iRow]["UNLOAD_MARK"].ToString();
                    strArray[iRow, 20] = dt.Rows[iRow]["WORK_NO"].ToString();
                    strArray[iRow, 21] = dt.Rows[iRow]["DANGER_GRADE"].ToString();
                    strArray[iRow, 22] = dt.Rows[iRow]["DEGREE_SETTING"].ToString();
                    strArray[iRow, 23] = dt.Rows[iRow]["DEGREE_UNIT"].ToString();
                    strArray[iRow, 24] = dt.Rows[iRow]["MIN_DEGREE"].ToString();
                    strArray[iRow, 25] = dt.Rows[iRow]["MAX_DEGREE"].ToString();
                    strArray[iRow, 26] = dt.Rows[iRow]["BAYNO"].ToString();
                    strArray[iRow, 27] = dt.Rows[iRow]["OLDBAYNO"].ToString();
                    strArray[iRow, 28] = dt.Rows[iRow]["CODE_CRANE"].ToString();

                    strArray[iRow, 29] = dt.Rows[iRow]["image_id"].ToString();
                    strArray[iRow, 30] = dt.Rows[iRow]["baynum"].ToString();
                    strArray[iRow, 31] = dt.Rows[iRow]["baycol"].ToString();
                    strArray[iRow, 32] = dt.Rows[iRow]["bayrow"].ToString();
                    strArray[iRow, 33] = dt.Rows[iRow]["container_no"].ToString();
                    strArray[iRow, 34] = dt.Rows[iRow]["size_con"].ToString();
                    strArray[iRow, 35] = dt.Rows[iRow]["container_type"].ToString();
                    strArray[iRow, 36] = dt.Rows[iRow]["code_empty"].ToString();
                    strArray[iRow, 37] = dt.Rows[iRow]["weight"].ToString();
                    strArray[iRow, 38] = dt.Rows[iRow]["work_date"].ToString();
                    strArray[iRow, 39] = dt.Rows[iRow]["sealno"].ToString();
                    strArray[iRow, 40] = dt.Rows[iRow]["moved_name"].ToString();
                    strArray[iRow, 41] = dt.Rows[iRow]["inoutmark"].ToString();
                    strArray[iRow, 42] = dt.Rows[iRow]["transmark"].ToString();
                    strArray[iRow, 43] = dt.Rows[iRow]["jjr"].ToString();
                    strArray[iRow, 44] = dt.Rows[iRow]["yb"].ToString();
                    strArray[iRow, 45] = dt.Rows[iRow]["name"].ToString();
                }

                Json = JsonConvert.SerializeObject(new DicPackage(true, strArray, null).DicInfo());
            }
            catch (Exception ex)
            {
                Json = JsonConvert.SerializeObject(new DicPackage(false, null, string.Format("{0}：获取某航次所有贝船图数据发生异常。{1}", ex.Source, ex.Message)).DicInfo());
            }
        }
        protected string Json;
    }
}