using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace M_Znlh.Statistics
{
    public class SingleStatisticsE
    {

        /// <summary>
        /// 姓名
        /// </summary>
        private string name = string.Empty;

        /// <summary>
        /// 20空
        /// </summary>
        private int e_20 = 0;

        /// <summary>
        /// 20空
        /// </summary>
        private int f_20 = 0;

        /// <summary>
        /// 40空
        /// </summary>
        private int e_40 = 0;

        /// <summary>
        /// 40重
        /// </summary>
        private int f_40 = 0;

        /// <summary>
        /// 其他空
        /// </summary>
        private int e_other = 0;

        /// <summary>
        /// F_other
        /// </summary>
        private int f_other = 0;

        public string Name
        {
            get
            {
                return name;
            }

            set
            {
                name = value;
            }
        }

        public int E_20
        {
            get
            {
                return e_20;
            }

            set
            {
                e_20 = value;
            }
        }

        public int F_20
        {
            get
            {
                return f_20;
            }

            set
            {
                f_20 = value;
            }
        }

        public int E_40
        {
            get
            {
                return e_40;
            }

            set
            {
                e_40 = value;
            }
        }

        public int F_40
        {
            get
            {
                return f_40;
            }

            set
            {
                f_40 = value;
            }
        }

        public int E_other
        {
            get
            {
                return e_other;
            }

            set
            {
                e_other = value;
            }
        }

        public int F_other
        {
            get
            {
                return f_other;
            }

            set
            {
                f_other = value;
            }
        }
    }
}