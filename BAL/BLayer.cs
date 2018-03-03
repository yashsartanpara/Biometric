using DAL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BAL
{
    public class BLayer
    {
        public class SelectClass
        {
            DLayer dl = new DLayer();
            public DataSet get_class()
            {
                DataSet ds = new DataSet();
                ds = dl.get_data("select * from ad_class");
                return ds;
            }
            public DataSet get_id_by_enroll_no(string enroll_no)
            {
                DataSet ds = new DataSet();
                ds = dl.get_data("select stu_id from student where enroll_no=" + enroll_no);
                return ds;
            }
            
            public DataSet get_enroll_no_by_id(int stu_id)
            {
                DataSet ds = new DataSet();
                ds = dl.get_data("select enroll_no from student where stu_id=" + stu_id);
                return ds;
            }
        }

    }
}
