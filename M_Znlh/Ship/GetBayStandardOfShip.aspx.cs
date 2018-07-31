//
//文件名：    GetBayStandardOfShip.aspx.cs
//功能描述：  获取某船贝位规范
//创建时间：  2016/11/30
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
    public partial class GetBayStandardOfShip : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            //身份校验
            if (!InterfaceTool.IdentityVerify(Request))
            {
                Json = JsonConvert.SerializeObject(new DicPackage(false, null, "身份认证错误！").DicInfo());
                return;
            }

            //船舶编码
            string strV_Id = Request.Params["strV_Id"];
            //strV_Id = "6945";


            try
            {
                if (strV_Id == null)
                {
                    Json = JsonConvert.SerializeObject(new DicPackage(false, null, "参数错误，获取某船贝位规范数据失败！").DicInfo());
                    return;
                }

                string strSql =
                    string.Format(@"select id,v_id,eng_vessel,chi_vessel,bay_num,board_col_count,board_col_mark,board_row_count,cabin_col_count,cabin_col_mark,cabin_row_count,joint,low_row_mark,board_unuse_col,cabin_unuse_col 
                                    from code_con_bay 
                                    WHERE v_id='{0}'",
                                    strV_Id);
                var dt = new Leo.Oracle.DataAccess(RegistryKey.KeyPathTally).ExecuteTable(strSql);
                if (dt.Rows.Count <= 0)
                {
                    Json = JsonConvert.SerializeObject(new DicPackage(true, null, "暂无数据！").DicInfo());
                    return;
                }

                string[,] strArray = new string[dt.Rows.Count, 12];
                for (int iRow = 0; iRow < dt.Rows.Count; iRow++)
                {
                    strArray[iRow, 0] = dt.Rows[iRow]["id"].ToString();
                    strArray[iRow, 1] = dt.Rows[iRow]["v_id"].ToString();
                    strArray[iRow, 2] = dt.Rows[iRow]["bay_num"].ToString();
                    strArray[iRow, 3] = dt.Rows[iRow]["board_col_count"].ToString();
                    strArray[iRow, 4] = dt.Rows[iRow]["board_col_mark"].ToString();
                    strArray[iRow, 5] = dt.Rows[iRow]["board_row_count"].ToString();
                    strArray[iRow, 6] = dt.Rows[iRow]["cabin_col_count"].ToString();
                    strArray[iRow, 7] = dt.Rows[iRow]["cabin_col_mark"].ToString();
                    strArray[iRow, 8] = dt.Rows[iRow]["cabin_row_count"].ToString();
                    strArray[iRow, 9] = dt.Rows[iRow]["joint"].ToString();
                    strArray[iRow, 10] = dt.Rows[iRow]["eng_vessel"].ToString();
                    strArray[iRow, 11] = dt.Rows[iRow]["chi_vessel"].ToString();
                }

                Json = JsonConvert.SerializeObject(new DicPackage(true, strArray, null).DicInfo());
            }
            catch (Exception ex)
            {
                Json = JsonConvert.SerializeObject(new DicPackage(false, null, string.Format("{0}：获取某船贝位规范数据发生异常。{1}", ex.Source, ex.Message)).DicInfo());
            }
        }
        protected string Json;
    }
}