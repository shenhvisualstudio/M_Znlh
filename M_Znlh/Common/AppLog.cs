//
//文件名：    AppLog.aspx.cs
//功能描述：  系统日志类
//创建时间：  2016/05/09
//作者：      
//修改时间：  暂无
//修改描述：  暂无
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Leo;

namespace ServiceInterface.Common
{
    /// <summary>
    /// 系统日志服务
    /// </summary>
    public class AppLog
    {
        //用户编码
        public string strCodeUser { get; set; }
        //App名称（简拼）
        public string strAppName { get; set; }
        //设备类型
        public string strDeviceType { get; set; }
        //IP
        public string strIP { get; set; }
        //操作行为
        public string strBehavior { get; set; }
        //操作行为URL（相对地址）
        public string strBehaviorURL { get; set; }
        //账号
        public string strAccount { get; set; }

        #region 公共方法
        /// <summary>
        /// 初始化系统日志数据
        /// </summary>
        public AppLog(HttpRequest Request)
        {
            strCodeUser = Request.Params["CodeUser"];
            strAppName = Request.Params["AppName"];
            strDeviceType = Request.ServerVariables["Http_User_Agent"].ToString();
            strIP = Request.ServerVariables.Get("Remote_Addr").ToString();
            strBehavior = null;
            strBehaviorURL = null;
            strAccount = null;
        }

        #region 记录操作成功日志
        /// <summary>
        /// 记录操作成功日志
        /// </summary>
        /// <param name="strRemark">信息备注（不超过200个字符）</param>
        public void LogCatalogSuccess(string strRemark)
        {
            string strSql =
                    string.Format(@"insert into TB_APP_LOG (code_user,appname,devicetype,ip,behavior,result,remark,behaviorurl,account)
                                    values('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}')",
                                    strCodeUser, strAppName, strDeviceType, strIP, strBehavior, "成功", strRemark, strBehaviorURL, strAccount);
            new Leo.Oracle.DataAccess(RegistryKey.KeyPathMa).ExecuteNonQuery(strSql);
        }

        /// <summary>
        /// 记录操作成功日志
        /// </summary>
        public void LogCatalogSuccess()
        {
            string strSql =
                    string.Format(@"insert into TB_APP_LOG (code_user,appname,devicetype,ip,behavior,result,remark,behaviorurl,account)
                                    values('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}')",
                                    strCodeUser, strAppName, strDeviceType, strIP, strBehavior, "成功", string.Empty, strBehaviorURL, strAccount);
            new Leo.Oracle.DataAccess(RegistryKey.KeyPathMa).ExecuteNonQuery(strSql);
        }
        #endregion

        #region 记录操作失败日志
        /// <summary>
        /// 记录操作失败日志
        /// </summary>
        /// <param name="strReason">失败原因（不超过200个字符）</param>
        public void LogCatalogFailure(string strReason)
        {
            string strSql =
                    string.Format(@"insert into TB_APP_LOG (code_user,appname,devicetype,ip,behavior,result,remark,behaviorurl,account)
                                    values('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}')",
                                    strCodeUser, strAppName, strDeviceType, strIP, strBehavior, "失败", strReason, strBehaviorURL, strAccount);
            new Leo.Oracle.DataAccess(RegistryKey.KeyPathMa).ExecuteNonQuery(strSql);
        }
        #endregion
        #endregion








    }
}