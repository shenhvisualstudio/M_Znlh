//
//文件名：    GetMovedSingleStatisticsTallyOfVoyage.aspx.cs
//功能描述：  获取捣箱个统计理货员数据
//创建时间：  2017/03/29
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
namespace M_Znlh.Statistics
{
    public partial class GetMovedSingleStatisticsTallyOfVoyage : System.Web.UI.Page
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
            //strShip_Id = "2014";


            try
            {
                if (strShip_Id == null)
                {
                    Json = JsonConvert.SerializeObject(new DicPackage(true, null, "参数错误，获取个统计理货员数据失败！").DicInfo());
                    return;
                }

                //strShip_Id = string.IsNullOrWhiteSpace(strShip_Id) ? string.Empty : strShip_Id;
            
                string strSql = string.Empty;
                string strTallySql = string.Empty;
                FullStatisticsE fullStatisticsE = new FullStatisticsE();


                strTallySql =
                    string.Format(@"select distinct(c.name) 
                                    from con_image i left join TALLY_CLERK c on c.work_no = i.work_no  
                                    where i.SHIP_ID = '{0}' AND MOVED = '1' AND c.NAME is not null",
                                    strShip_Id);

                strSql =
                    string.Format(@"select SIZE_CON,FULLOREMPTY,c.name,count(case when unload_mark='1' then 1 end) tallyCon 
                                    from con_image i left join TALLY_CLERK c on c.work_no=i.work_no  
                                    where i.SHIP_ID='{0}' AND MOVED='1'  
                                    group by size_con,fullorempty,c.name",
                                    strShip_Id);


                var dt = new Leo.Oracle.DataAccess(RegistryKey.KeyPathTally).ExecuteTable(strTallySql);
                if (dt.Rows.Count <= 0)
                {
                    Json = JsonConvert.SerializeObject(new DicPackage(true, null, "暂无数据！").DicInfo());
                    return;
                }

                List<SingleStatisticsE> singleStatisticsList = new List<SingleStatisticsE>();
            
                for (int iRow = 0; iRow < dt.Rows.Count; iRow++)
                {
                    SingleStatisticsE singleStatisticsE = new SingleStatisticsE();
                    singleStatisticsE.Name = Convert.ToString(dt.Rows[iRow]["NAME"]);
                    singleStatisticsList.Add(singleStatisticsE);
                }

                dt = new Leo.Oracle.DataAccess(RegistryKey.KeyPathTally).ExecuteTable(strSql);


                for (int iRow = 0; iRow < dt.Rows.Count; iRow++)
                {
                    string name = Convert.ToString(dt.Rows[iRow]["NAME"]);

                    for (int j = 0; j < singleStatisticsList.Count; j++)
                    {
                        SingleStatisticsE s = singleStatisticsList[j];

                        if (s.Name.Equals(name))
                        {

                            if (dt.Rows[iRow]["FULLOREMPTY"].Equals("E"))
                            {
                                if (dt.Rows[iRow]["SIZE_CON"].Equals("20"))
                                {
                                    s.E_20 = Convert.ToInt16(dt.Rows[iRow]["TALLYCON"]);

                                }
                                else if (dt.Rows[iRow]["SIZE_CON"].Equals("40"))
                                {
                                    s.E_40 = Convert.ToInt16(dt.Rows[iRow]["TALLYCON"]);
                                }
                                else
                                {
                                    s.E_other = Convert.ToInt16(dt.Rows[iRow]["TALLYCON"]);
                                }
                            }
                            if (dt.Rows[iRow]["FULLOREMPTY"].Equals("F"))
                            {
                                if (dt.Rows[iRow]["SIZE_CON"].Equals("20"))
                                {
                                    s.F_20 = Convert.ToInt16(dt.Rows[iRow]["TALLYCON"]);
                                }
                                else if (dt.Rows[iRow]["SIZE_CON"].Equals("40"))
                                {
                                    s.F_40 = Convert.ToInt16(dt.Rows[iRow]["TALLYCON"]);
                                }
                                else
                                {
                                    s.F_other = Convert.ToInt16(dt.Rows[iRow]["TALLYCON"]);
                                }
                            }
                        }
                    }

                }


                Json = JsonConvert.SerializeObject(new DicPackage(true, singleStatisticsList, null).DicInfo());
            }
            catch (Exception ex)
            {
                Json = JsonConvert.SerializeObject(new DicPackage(false, null, string.Format("{0}：获取个统计理货员数据发生异常。{1}", ex.Source, ex.Message)).DicInfo());
            }
        }
        protected string Json;
    }
}