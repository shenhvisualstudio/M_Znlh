//
//文件名：    DicPackage.aspx.cs
//功能描述：  交换数据包（返回值）
//创建时间：  2015/07/09
//作者：      
//修改时间：  暂无
//修改描述：  暂无
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ServiceInterface.Common
{
    public class DicPackage
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        private bool isSuccess;
        /// <summary>
        /// 数据
        /// </summary>
        private object data;
        /// <summary>
        /// 消息（失败提示、异常）
        /// </summary>
        private object message;

        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="message">消息</param>
        public DicPackage(bool isSuccess, object data, object message)
        {
            this.isSuccess = isSuccess;
            this.data = data;
            this.message = message;
        }

        /// <summary>
        /// 字典交换数据包
        /// </summary>
        /// <param name="data">数据集</param>
        /// <returns>Dictionary对象</returns>
        public Dictionary<string, object> DicInfo()
        {
            Dictionary<string, object> info = new Dictionary<string, object>();
            if (this.isSuccess == true)
            {
                info.Add("IsSuccess", true);
            }
            else
            {
                info.Add("IsSuccess", false);
            }

            info.Add("Data", this.data);
            info.Add("Message", this.message);
            return info;
        }
    }
}