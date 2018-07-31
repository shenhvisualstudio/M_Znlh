﻿//
//文件名：    GetSingleStatisticsOperateOfVoyage.aspx.cs
//功能描述：  获取个统计操作员数据
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
    public partial class GetSingleStatisticsOperateOfVoyage : System.Web.UI.Page
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
            //进出口编码
            string strCodeInOut = Request.Params["CodeInOut"];
            //strShip_Id = "2014";
            //strCodeInOut = "0";


            try
            {
                if (strShip_Id == null || strCodeInOut == null)
                {
                    Json = JsonConvert.SerializeObject(new DicPackage(true, null, "参数错误，获取个统计操作员数据数据失败！").DicInfo());
                    return;
                }

                //strShip_Id = string.IsNullOrWhiteSpace(strShip_Id) ? string.Empty : strShip_Id;
                //strCodeInOut = string.IsNullOrWhiteSpace(strCodeInOut) ? string.Empty : strCodeInOut;

                string strSql = string.Empty;
                string strTallySql = string.Empty;
                FullStatisticsE fullStatisticsE = new FullStatisticsE();


                //进
                if (strCodeInOut.Equals("0"))
                {

                    strTallySql =
                        string.Format(@"select distinct(OPER_NAME) 
                                        FROM (SELECT I.SIZE_CON,I.FULLOREMPTY,I.UNLOAD_MARK,I.CONTAINER_NO,CASE WHEN T.NAME IS NOT NULL THEN  T.NAME ELSE I.USER_NAME END as OPER_NAME
                                        from con_image I LEFT JOIN TALLY_CLERK  T ON  I.USER_NAME=T.WORK_NO
                                        where I.SHIP_ID='{0}' and (I.CODE_UNLOAD_PORT LIKE '%LYG' or MOVED='1') AND I.USER_NAME is not null)
                                        group by size_con,fullorempty,OPER_NAME",
                                        strShip_Id);

                    strSql =
                        string.Format(@"select SIZE_CON,FULLOREMPTY,OPER_NAME,count(case when unload_mark='1' then 1 end) operCon
                                        FROM (SELECT I.SIZE_CON,I.FULLOREMPTY,I.UNLOAD_MARK,I.CONTAINER_NO,CASE WHEN T.NAME IS NOT NULL THEN  T.NAME ELSE I.USER_NAME END as OPER_NAME
                                        from con_image I LEFT JOIN TALLY_CLERK  T ON  I.USER_NAME=T.WORK_NO
                                        where I.SHIP_ID='{0}' and (I.CODE_UNLOAD_PORT LIKE '%LYG' or MOVED='1'))
                                        group by size_con,fullorempty,OPER_NAME",
                                        strShip_Id);

                }
                else
                {

                    strTallySql =
                        string.Format(@"select distinct(OPER_NAME) 
                                        FROM (SELECT I.SIZE_CON,I.FULLOREMPTY,I.UNLOAD_MARK,I.CONTAINER_NO,CASE WHEN T.NAME IS NOT NULL THEN  T.NAME ELSE I.USER_NAME END as OPER_NAME
                                        from con_image I LEFT JOIN TALLY_CLERK  T ON  I.USER_NAME=T.WORK_NO
                                        where I.SHIP_ID='{0}' and (I.CODE_LOAD_PORT LIKE '%LYG' or MOVED='1') AND I.USER_NAME is not null)
                                        group by size_con,fullorempty,OPER_NAME",
                                        strShip_Id);

                    strSql =
                        string.Format(@"select SIZE_CON,FULLOREMPTY,OPER_NAME,count(case when unload_mark='1' then 1 end) operCon
                                        FROM (SELECT I.SIZE_CON,I.FULLOREMPTY,I.UNLOAD_MARK,I.CONTAINER_NO,CASE WHEN T.NAME IS NOT NULL THEN  T.NAME ELSE I.USER_NAME END as OPER_NAME
                                        from con_image I LEFT JOIN TALLY_CLERK  T ON  I.USER_NAME=T.WORK_NO
                                        where I.SHIP_ID='{0}' and (I.CODE_LOAD_PORT LIKE '%LYG' or MOVED='1'))
                                        group by size_con,fullorempty,OPER_NAME",
                                        strShip_Id);
                }

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
                    singleStatisticsE.Name = Convert.ToString(dt.Rows[iRow]["OPER_NAME"]);
                    singleStatisticsList.Add(singleStatisticsE);
                }

                dt = new Leo.Oracle.DataAccess(RegistryKey.KeyPathTally).ExecuteTable(strSql);


                for (int iRow = 0; iRow < dt.Rows.Count; iRow++)
                {
                    string name = Convert.ToString(dt.Rows[iRow]["OPER_NAME"]);

                    for (int j = 0; j < singleStatisticsList.Count; j++)
                    {
                        SingleStatisticsE s = singleStatisticsList[j];

                        if (s.Name.Equals(name))
                        {

                            if (dt.Rows[iRow]["FULLOREMPTY"].Equals("E"))
                            {
                                if (dt.Rows[iRow]["SIZE_CON"].Equals("20"))
                                {
                                    s.E_20 = Convert.ToInt16(dt.Rows[iRow]["OPERCON"]);

                                }
                                else if (dt.Rows[iRow]["SIZE_CON"].Equals("40"))
                                {
                                    s.E_40 = Convert.ToInt16(dt.Rows[iRow]["OPERCON"]);
                                }
                                else
                                {
                                    s.E_other = Convert.ToInt16(dt.Rows[iRow]["OPERCON"]);
                                }
                            }
                            if (dt.Rows[iRow]["FULLOREMPTY"].Equals("F"))
                            {
                                if (dt.Rows[iRow]["SIZE_CON"].Equals("20"))
                                {
                                    s.F_20 = Convert.ToInt16(dt.Rows[iRow]["OPERCON"]);
                                }
                                else if (dt.Rows[iRow]["SIZE_CON"].Equals("40"))
                                {
                                    s.F_40 = Convert.ToInt16(dt.Rows[iRow]["OPERCON"]);
                                }
                                else
                                {
                                    s.F_other = Convert.ToInt16(dt.Rows[iRow]["OPERCON"]);
                                }
                            }
                        }
                    }

                }


                Json = JsonConvert.SerializeObject(new DicPackage(true, singleStatisticsList, null).DicInfo());
            }
            catch (Exception ex)
            {
                Json = JsonConvert.SerializeObject(new DicPackage(false, null, string.Format("{0}：获取个统计操作员数据发生异常。{1}", ex.Source, ex.Message)).DicInfo());
            }
        }
        protected string Json;
    }
}