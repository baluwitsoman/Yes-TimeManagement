using AppCode;
using CommonObjects;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using Telerik.Web.Design;
using Telerik.Web.UI;

public partial class Job_Card_Details : ERPPage
{
    dbaccess obj_db = new dbaccess();
    dbAccessXML obj_db_xml = new dbAccessXML();
    string str_current_user_id = "10";
    protected string str_company_code = "";
    DataTable dt_details = new DataTable("dt_details");
    UserDatail userDetail = null;
    public string IsActiveFleet = "N";

    protected void Page_Load(object sender, EventArgs e)
    {
        if (Session["userDetail"] != null)
        {
            userDetail = (UserDatail)Session["userDetail"];
        }
        str_current_user_id = userDetail.User_Id.ToString();
        str_company_code = userDetail.Login_Company_Code;
        Master.Page.Title = "Project/JobCard Master";
        lblError.Visible = false;
        txtjobcode.Attributes.Add("readonly","readonly");

        //if (!IsPostBack)
        //{
        //    MTR_PARTY_IND.SelectedValue = "AR";
        //}

        //MTR_PARTY_IND.SelectedIndexChanged += new EventHandler(MTR_PARTY_IND_SelectedIndexChanged);

        this.PreRender += new EventHandler(Logistics_ProjectMaster_PreRender);

        // MTR_REF_CODE.Attributes.Add("readonly", "readonly");

      //  PopupWithEmpCode.TextBoxDesc.Attributes.Add("readonly", "readonly");
       // txtEmpname.Attributes.Add("readonly", "readonly");

        if (!IsPostBack)
        { 
            Binddataloading(); 
           
           // txtEmpname.Text = userDetail._Emp_Name;
           // PopupWithEmpCode.TextBoxCode.Value = PopupWithEmpCode.TextBoxDesc.Text = userDetail.Emp_Code;
           // MTR_REF_DATE.Text = DateTime.Now.ToString("dd/MM/yyyy");

        }
         
       // populateJobcard();
    }

    protected void Emp_indexchanged(object sender, EventArgs e)
    {
        populateemp();
    }

    void populateemp()
    {
        string strparty = "SELECT PEMP_EMP_CODE,PEMP_EMP_NAME,PBM_BRANCH_NAME,PDPM_DEPARTMENT_DESC,PEMP_EMP_DESIGNATION_DESC,PEMP_EMP_PHONE FROM V_EMPLOYEE_DETAILS WHERE PEMP_EMP_ACTIVE='Y' AND PEMP_EMP_CODE='"+txtEmpId.Text+"'";
        DataTable DTEmp = new DataTable();
        DTEmp = obj_db.execute_query_retun_datatable(strparty);
        if (DTEmp.Rows.Count > 0)
        {
            txtEmpId.Text = DTEmp.Rows[0]["PEMP_EMP_NAME"].ToString();
            hid_emp_code.Value = DTEmp.Rows[0]["PEMP_EMP_CODE"].ToString();
        }
    }

    protected void Party_indexchanged(object sender, EventArgs e)
    {
        populateparty();
    }
    void populateparty()
    {
        string strparty = "SELECT   MPD_BRANCH_CODE, MPD_BRANCH_NAME  FROM  MMM_PARTY_DETAILS WHERE MPD_COMP_CODE='" + str_company_code + "' AND MPD_PARTY_TYPE='" + MTRPARTYIND.Value + "' AND MPD_BRANCH_CODE='" + MTR_PARTY_CODE.Text + "' ";
        DataTable DTParty = new DataTable();
        DTParty = obj_db.execute_query_retun_datatable(strparty);
        if (DTParty.Rows.Count > 0)
        {
            MTR_PARTY_CODE.Text = DTParty.Rows[0]["MPD_BRANCH_NAME"].ToString();
            MTRPARTYNAME.Value = DTParty.Rows[0]["MPD_BRANCH_CODE"].ToString();
        }
    }
    protected void jobcode_indexchanged(object sender, EventArgs e)
    {
        populateJobcard();
    }

    void populateJobcard()
    {
      string  str_query = @"SELECT   MTJ_COMP_CODE, MTJ_PARTY_IND,MTJ_PARTY_CODE,MPD_BRANCH_CODE, MPD_BRANCH_NAME,MTJ_JOB_LOCATION,MTJ_BRAND,MTJ_EQUIPMENT_TYPE_ID,MTJ_SERVICE_TYPE_ID,MTJ_SERIAL_NO,MTJ_JOB_CODE,MTJ_JOB_DESC,
        TO_char(MTJ_JOB_OPENING_DATE, 'dd/MM/yyyy') MTJ_JOB_OPENING_DATE,TO_char(MTJ_JOB_SERVICE_START_DATE, 'dd/MM/yyyy') MTJ_JOB_SERVICE_START_DATE,TO_char(MTJ_JOB_CLOSING_DATE, 'dd/MM/yyyy') MTJ_JOB_CLOSING_DATE,MTJ_JOB_STATUS,
        MTJ_CONTACT_PERSON,MTJ_CONTACT_PHONE,MTJ_SALES_EMPLOYEE_CODE,PEMP_EMP_CODE,PEMP_EMP_NAME,MTJ_SALES_EMPLOYEE_PHONE,MTJ_BRANCH,MTJ_SECTOR,MTJ_REMARKS
         FROM MTL_TRANSACTION_JOB_OTHER_DTLS LEFT  JOIN V_EMPLOYEE_DETAILS ON  MTJ_SALES_EMPLOYEE_CODE = PEMP_EMP_CODE , MMM_PARTY_DETAILS  
    WHERE   MTJ_PARTY_CODE = MPD_BRANCH_CODE  AND MPD_PARTY_TYPE = 'AR'
    AND   MTJ_JOB_CODE='" + hidjobcode.Value+"'";

        DataTable DTjobcard = new DataTable();
        DTjobcard = obj_db.execute_query_retun_datatable(str_query);
        if (DTjobcard.Rows.Count > 0)
        {
            hidjobcode.Value= DTjobcard.Rows[0]["MTJ_JOB_CODE"].ToString();
            txtjobcode.Text = DTjobcard.Rows[0]["MTJ_JOB_CODE"].ToString();
            txtjobdesc.Text = DTjobcard.Rows[0]["MTJ_JOB_DESC"].ToString();
            txtjobopeningdate.Text = DTjobcard.Rows[0]["MTJ_JOB_OPENING_DATE"].ToString();
            txtjobservicedate.Text = DTjobcard.Rows[0]["MTJ_JOB_SERVICE_START_DATE"].ToString();
            txtjobclosingdate.Text = DTjobcard.Rows[0]["MTJ_JOB_CLOSING_DATE"].ToString();


            // ddljobstatus.SelectedValue= DTjobcard.Rows[0]["MTJ_JOB_STATUS"].ToString();
            string jobStatus = DTjobcard.Rows[0]["MTJ_JOB_STATUS"].ToString();

            if (!string.IsNullOrEmpty(jobStatus) &&
                ddljobstatus.Items.FindByValue(jobStatus) != null)
            {
                ddljobstatus.SelectedValue = jobStatus;
            }

            MTRPARTYNAME.Value = DTjobcard.Rows[0]["MTJ_PARTY_CODE"].ToString();
            MTRPARTYIND.Value  = DTjobcard.Rows[0]["MTJ_PARTY_IND"].ToString();
            MTR_PARTY_CODE.Text = DTjobcard.Rows[0]["MPD_BRANCH_NAME"].ToString();

            if (DTjobcard.Rows[0]["MTJ_JOB_LOCATION"].ToString() == "Field")
                rdljobfield.Checked = true;
            else if (DTjobcard.Rows[0]["MTJ_JOB_LOCATION"].ToString() == "Service")
                rdlService.Checked = true;
            else if (DTjobcard.Rows[0]["MTJ_JOB_LOCATION"].ToString() == "Parts")
                rdlParts.Checked = true;
            else

                rdljobworkshop.Checked = true;

          //  ddlbrandlist.SelectedValue = DTjobcard.Rows[0]["MTJ_BRAND"].ToString();


            string brandValue = DTjobcard.Rows[0]["MTJ_BRAND"].ToString();

            if (!string.IsNullOrEmpty(brandValue) &&
                ddlbrandlist.Items.FindByValue(brandValue) != null)
            {
                ddlbrandlist.SelectedValue = brandValue;
            }

            ddlequipment.SelectedValue= DTjobcard.Rows[0]["MTJ_EQUIPMENT_TYPE_ID"].ToString();
            ddlservicetype.SelectedValue = DTjobcard.Rows[0]["MTJ_SERVICE_TYPE_ID"].ToString();
            txtmachinegensetno.Text = DTjobcard.Rows[0]["MTJ_SERIAL_NO"].ToString();

            txtcustomercontact.Text = DTjobcard.Rows[0]["MTJ_CONTACT_PERSON"].ToString();
            txtcustphoneno.Text = DTjobcard.Rows[0]["MTJ_CONTACT_PHONE"].ToString();
            hid_emp_code.Value = DTjobcard.Rows[0]["MTJ_SALES_EMPLOYEE_CODE"].ToString();
             txtEmpId.Text = DTjobcard.Rows[0]["PEMP_EMP_NAME"].ToString();
            txtsalesmancontact.Text = DTjobcard.Rows[0]["MTJ_SALES_EMPLOYEE_PHONE"].ToString();
            ddlbranch.DataBind();

            string branchValue = DTjobcard.Rows[0]["MTJ_BRANCH"].ToString();

            if (!string.IsNullOrEmpty(branchValue) &&
                ddlbranch.Items.FindByValue(branchValue) != null)
            {
                ddlbranch.SelectedValue = branchValue;
            }

            txtsector.Text = DTjobcard.Rows[0]["MTJ_SECTOR"].ToString();
            txtremarks.Text = DTjobcard.Rows[0]["MTJ_REMARKS"].ToString();
        }

    }
    void Binddataloading()
    {
        string strquery1 = "SELECT 0 MET_EQUIPMENT_TYPE_ID,'SELECT' MET_EQUIPMENT_TYPE_DESC FROM DUAL UNION ALL SELECT T.* FROM (SELECT MET_EQUIPMENT_TYPE_ID,MET_EQUIPMENT_TYPE_DESC FROM MMM_EQUIPMENT_TYPE ORDER BY MET_SORT_ORDER)T";
        DataTable DTEquip = new DataTable();
        DTEquip = obj_db.execute_query_retun_datatable(strquery1);
        ddlequipment.DataSource = DTEquip;      
        ddlequipment.DataValueField = "MET_EQUIPMENT_TYPE_ID";
        ddlequipment.DataTextField = "MET_EQUIPMENT_TYPE_DESC";       
        ddlequipment.DataBind();
 

        string strquery2 = " SELECT 0 MST_SERVICE_TYPE_ID,'SELECT' MST_SERVICE_TYPE_DESC FROM DUAL UNION ALL SELECT T.* FROM ( SELECT MST_SERVICE_TYPE_ID,MST_SERVICE_TYPE_DESC FROM MMM_SERVICE_TYPE ORDER BY MST_SORT_ORDER)T";
        DataTable DTService = new DataTable();
        DTService = obj_db.execute_query_retun_datatable(strquery2);
        ddlservicetype.DataSource = DTService;
        ddlservicetype.DataValueField = "MST_SERVICE_TYPE_ID";
        ddlservicetype.DataTextField = "MST_SERVICE_TYPE_DESC";
        ddlservicetype.DataBind();
 

        string strquery3 = " SELECT '' MCMD_ENTITY_CODE,'SELECT' MCMD_ENTITY_DESC FROM DUAL UNION ALL SELECT T.* FROM ( select MCMD_ENTITY_CODE,MCMD_ENTITY_DESC from MMM_COMMON_MASTERS_DETAIL    WHERE  UPPER(MCMD_ENTITY_GROUP) = 'AGENCY' order by MCMD_ENTITY_DESC)T";
        DataTable DTBrand = new DataTable();
        DTBrand = obj_db.execute_query_retun_datatable(strquery3);
        ddlbrandlist.DataSource = DTBrand;
        ddlbrandlist.DataValueField = "MCMD_ENTITY_CODE";
        ddlbrandlist.DataTextField = "MCMD_ENTITY_DESC";
        ddlbrandlist.DataBind();
 

        string strquery4 = " SELECT '' PBM_BRANCH_CODE,'SELECT' PBM_BRANCH_NAME FROM DUAL UNION ALL SELECT T.* FROM ( SELECT PBM_BRANCH_CODE, PBM_BRANCH_NAME FROM PPM_BRANCH_MASTER ORDER BY PBM_SORT_ORDER)T";
        DataTable DTBranch = new DataTable();
        DTBranch = obj_db.execute_query_retun_datatable(strquery4);
        ddlbranch.DataSource = DTBranch;
        ddlbranch.DataValueField = "PBM_BRANCH_CODE";
        ddlbranch.DataTextField = "PBM_BRANCH_NAME";
        ddlbranch.DataBind();

        string strquery5 = " SELECT '' MCMD_ENTITY_CODE,'SELECT' MCMD_ENTITY_DESC FROM DUAL UNION ALL SELECT T.* FROM ( select MCMD_ENTITY_CODE,MCMD_ENTITY_DESC from MMM_COMMON_MASTERS_DETAIL    WHERE  UPPER(MCMD_ENTITY_GROUP) = 'JOB_CARD_STATUS' order by MCMD_ENTITY_DESC)T";
        DataTable DTJobStatus = new DataTable();
        DTJobStatus = obj_db.execute_query_retun_datatable(strquery5);
        ddljobstatus.DataSource = DTJobStatus;
        ddljobstatus.DataValueField = "MCMD_ENTITY_CODE";
        ddljobstatus.DataTextField = "MCMD_ENTITY_DESC";
        ddljobstatus.DataBind();

    }
    void Logistics_ProjectMaster_PreRender(object sender, EventArgs e)
    {

        //divFleetRow.Visible = divPartyRow.Visible = false;

        //if (MTR_PARTY_IND.SelectedValue == "FL")
        //    divFleetRow.Visible = true;
        //else
        //    divPartyRow.Visible = true;



    }

    protected void MTR_PARTY_IND_SelectedIndexChanged(object sender, EventArgs e)
    {

    }

    


    protected void Button1_Click(object sender, EventArgs e)
    {

    }

    private void insert_update()
    {
        string str_selected_channel_code = hidjobcode.Value.Trim().Replace("'", "''");
        string str_result = obj_db.execute_scalar("SELECT COUNT(*) FROM MTL_TRANSACTION_JOB_OTHER_DTLS WHERE UPPER(MTJ_JOB_CODE) =UPPER('" + str_selected_channel_code + "') AND MTJ_COMP_CODE='" + str_company_code + "' ");
        log_error.write_to_log_file("check", "false", str_result);
        int i_menu_exist = 0; int cnt_code = 0;
        if (str_result != "")
        {
            i_menu_exist = int.Parse(str_result);
        }
        Page.Validate();
        // check client validation is failed or not
        if (!Page.IsValid)
        {
            //lblError.Text = "The input values are not valid. ";
            //lblError.Visible = true;
            CommonTasks.WarningMessage("The input values are not valid", Page);
            return;
        }
        
        if (txtjobdesc.Text.Length > 200)
        {
            CommonTasks.WarningMessage("Description total characters cant be more than 200.", Page);
            return;
        }
        if (txtremarks.Text.Length>1000)
        {
            CommonTasks.WarningMessage("Remarks total characters cant be more than 1000.", Page);
            return;
        }
          cnt_code = obj_db.record_found("select count(*) from MTL_TRANSACTION_JOB_OTHER_DTLS");
        if(i_menu_exist==0)
        {

            //if (cnt_code > 0)
            //{
            //    cnt_code = obj_db.execute_scalar("select MAX(MTJ_JOB_CODE) from MTL_TRANSACTION_JOB_OTHER_DTLS").ToString().ToInt() + 1;
            //    txtjobcode.Text = "000" + cnt_code.ToString();
            //}
            //else
            //{
            //    txtjobcode.Text = "0001";

            //}
            var docSysId = obj_db.execute_scalar(@"SELECT 
MDCH_SYS_ID  
FROM  MMM_DOCUMENT_CONTROL_HEADER where  MDCH_TRANS_CODE = 'JCD' and  MDCH_DOC_CODE ='JCD' ");
            List<ERP1.Pro_Parameters> parameters = new List<ERP1.Pro_Parameters>();
            parameters.Add(new ERP1.Pro_Parameters
            {
                _par_name = "P_DOC_SYS_ID",
                _par_value = docSysId.ToString().ToInt(),
                _par_in_out = "IN"
            });
            parameters.Add(new ERP1.Pro_Parameters { _par_name = "P_TRN_DT", _par_value = DateTime.Now });
            parameters.Add(new ERP1.Pro_Parameters { _par_name = "P_NUMBER", _par_value = "", _par_in_out = "OUT", _par_data_type = "Varchar2" });
            var sBAccess = new ERP1.DBAccess();

            sBAccess.executeProcedure("PRO_GET_NEXT_DOCNO", parameters);

            hidjobcode.Value= txtjobcode.Text = parameters[2]._par_value.ToString();
        }

        string str_chk_location = "";
        if (rdljobfield.Checked == true)
            str_chk_location = "Field";
        else if (rdlService.Checked == true)
            str_chk_location = "Service";
        else if (rdlParts.Checked == true)
            str_chk_location = "Parts";
        else
            str_chk_location = "Workshop";
        string str_insert_update_query = "";
        // already exist, update for detail
        if (i_menu_exist>0)
        {
            str_insert_update_query = @" UPDATE MTL_TRANSACTION_JOB_OTHER_DTLS
                                        SET    MTJ_PARTY_CODE       = '" + MTRPARTYNAME.Value.Replace("'", "''") + @"',
                                        MTJ_JOB_LOCATION='"+ str_chk_location + @"', 
                                        MTJ_BRAND = '" + ddlbrandlist.SelectedValue + @"',
                                         MTJ_EQUIPMENT_TYPE_ID = '" + ddlequipment.SelectedValue + @"',
                                         MTJ_SERVICE_TYPE_ID = '" + ddlservicetype.SelectedValue + @"',
                                         MTJ_SERIAL_NO = '" + txtmachinegensetno.Text.Trim().Replace("'", "") + @"',
                                         MTJ_JOB_DESC = '" + txtjobdesc.Text.Trim().Replace("'","") + @"',
                                         MTJ_JOB_OPENING_DATE       = TO_DATE('" + txtjobopeningdate.Text.Trim().Replace("'", "''") + @"','dd/MM/yyyy'),
                                         MTJ_JOB_SERVICE_START_DATE       = TO_DATE('" + txtjobservicedate.Text.Trim().Replace("'", "''") + @"','dd/MM/yyyy'),
                                         MTJ_JOB_CLOSING_DATE       = TO_DATE('" + txtjobclosingdate.Text.Trim().Replace("'", "''") + @"','dd/MM/yyyy'),
                                        MTJ_JOB_STATUS = '" + ddljobstatus.SelectedValue + @"',
                                        MTJ_CONTACT_PERSON = '" + txtcustomercontact.Text + @"',
                                        MTJ_CONTACT_PHONE = '" + txtcustphoneno.Text + @"',                  
                                        MTJ_SALES_EMPLOYEE_CODE = '" + hid_emp_code.Value + @"',
                                        MTJ_SALES_EMPLOYEE_PHONE = '" + txtsalesmancontact.Text + @"',
                                        MTJ_BRANCH = '" + ddlbranch.SelectedValue + @"',
                                        MTJ_SECTOR = '" + txtsector.Text + @"',                                      
                                        MTJ_REMARKS      = '" + txtremarks.Text.Replace("'","") + @"',                                       
                                       MTJ_UPDATE_USER_ID = '" + userDetail.User_Id + @"',
                                       MTJ_UPDATE_DATE    =SYSDATE 
                                       WHERE  UPPER(MTJ_JOB_CODE) = UPPER('" + hidjobcode.Value.Trim().Replace("'", "''") + @"') AND MTJ_COMP_CODE='" + str_company_code + "' ";

                            log_error.write_to_log_file("job card", "update", str_insert_update_query);
                                  
                                        
                                        if (obj_db.execute_query(str_insert_update_query) > 0)
                                        {
                                            CommonTasks.SuccessMessage("The record updated successfully", Page);
                                            populateJobcard();
                                        }                                           
                                        else
                                            CommonTasks.WarningMessage("Unable to save. Please contact admin.", Page);

        }
 
        else
        {


            str_insert_update_query = @"
        INSERT INTO MTL_TRANSACTION_JOB_OTHER_DTLS
    (MTJ_COMP_CODE,MTJ_PARTY_IND, MTJ_PARTY_CODE,MTJ_JOB_LOCATION,MTJ_BRAND,MTJ_EQUIPMENT_TYPE_ID,MTJ_SERVICE_TYPE_ID,MTJ_SERIAL_NO, 
    MTJ_JOB_CODE, MTJ_JOB_DESC, MTJ_JOB_OPENING_DATE, MTJ_JOB_SERVICE_START_DATE,MTJ_JOB_CLOSING_DATE,MTJ_JOB_STATUS,MTJ_CONTACT_PERSON,MTJ_CONTACT_PHONE,
    MTJ_SALES_EMPLOYEE_CODE,MTJ_SALES_EMPLOYEE_PHONE,MTJ_BRANCH,MTJ_SECTOR,MTJ_REMARKS,MTJ_CREATION_USER_ID,MTJ_CREATION_DATE)VALUES 
    (
        '" + str_company_code + "','" + MTRPARTYIND.Value.Trim().Replace("'", "''") + @"' ,'" + MTRPARTYNAME.Value.Trim().Replace("'", "''") +
            @"','" + str_chk_location + @"' , '" + ddlbrandlist.SelectedValue + @"','" + ddlequipment.SelectedValue +
            @"','" + ddlservicetype.SelectedValue + @"','" + txtmachinegensetno.Text.Trim() + @"'  ,'" + txtjobcode.Text +
            @"','" + txtjobdesc.Text.Replace("'", "") + @"',";

            if(txtjobopeningdate.Text!="")
                str_insert_update_query +=@" TO_DATE('" + txtjobopeningdate.Text.Trim().Replace("'", "''") + @"','dd/MM/yyyy'),";
            else
                str_insert_update_query += "'',";

            if (txtjobservicedate.Text != "")
                str_insert_update_query += @"TO_DATE('" + txtjobservicedate.Text.Trim().Replace("'", "''") + @"','dd/MM/yyyy') ,";
            else
                str_insert_update_query += "'',";

            if (txtjobclosingdate.Text != "")
                str_insert_update_query += @" TO_DATE('" + txtjobclosingdate.Text.Trim().Replace("'", "''") + @"','dd/MM/yyyy'),";
            else
                str_insert_update_query += "'',";

            str_insert_update_query += @"'" + ddljobstatus.SelectedValue + @"','" + txtcustomercontact.Text.Trim().Replace("'","")+ @"','" + txtcustphoneno.Text.Trim().Replace("'", "") + @"','" + hid_emp_code.Value + @"','" + txtsalesmancontact.Text + @"','" + ddlbranch.SelectedValue + @"','" + txtsector.Text + @"','" + txtremarks.Text.Replace("'", "") + @"'," + userDetail.User_Id + @",SYSDATE)";


            log_error.write_to_log_file("job card", "insert", str_insert_update_query);
            //lblError.Text = "The record saved successfully.";
            if (obj_db.execute_query(str_insert_update_query) > 0)
            {
                CommonTasks.SuccessMessage("The record saved successfully", Page);
                populateJobcard();
            }
            else
            {
                CommonTasks.WarningMessage("Unable to save. Please contact admin.", Page);

            }
        }
        

         lblError.Visible = true;
    }


    private void clear_form_view()
    {
        MTR_PARTY_CODE.Text = "";MTRPARTYNAME.Value = "";
        rdljobfield.Checked = true;
        ddlbrandlist.SelectedValue = "";
        ddlequipment.SelectedValue = "0";
        ddlservicetype.SelectedValue = "0";
        txtmachinegensetno.Text = "";
        txtjobcode.Text = ""; 
        txtjobdesc.Text = "";
        txtjobopeningdate.Text = "";
        txtjobservicedate.Text = "";
        txtjobclosingdate.Text = "";
        ddljobstatus.SelectedValue = "";
        txtcustomercontact.Text = "";
        txtcustphoneno.Text = "";
        hidjobcode.Value = "";
        txtEmpId.Text = "";hid_emp_code.Value = "";
        txtsalesmancontact.Text = "";
        ddlbranch.SelectedValue = "";
        txtsector.Text = "";
        txtremarks.Text = "";

    }


    protected void btnSave_Click(object sender, EventArgs e)
    {
        insert_update();
    }
    protected void btnReport_Click(object sender, EventArgs e)
    {




    }
    protected void btnDelete_Click(object sender, EventArgs e)
    {
        string str_delete_query = "";
        if (obj_db.record_found(@"SELECT COUNT(1) FROM MTL_TRANSACTIONS_HEADER WHERE MTH_NARR5 = '" + hidjobcode.Value.Trim() + @"'") > 0)
        {
            //lblError.Text = "This Project/Job/Reference has dependecies. You shouldnt delete now.";
            //lblError.Visible = true;
            CommonTasks.WarningMessage("This Job card has dependecies. You shouldnt delete now", Page);
            return;
        }
        if (hidjobcode.Value != "")
        {
            str_delete_query = "DELETE  FROM MTL_TRANSACTION_JOB_OTHER_DTLS WHERE upper(MTJ_JOB_CODE)='" + hidjobcode.Value.Trim().Replace("'", "''").ToUpper() + "'";
            if (obj_db.execute_query(str_delete_query) >= 1)
            {
                //lblError.Text = "The records deleted successfully.";
                //lblError.Visible = true;
                CommonTasks.WarningMessage("The records deleted successfully", Page);
                clear_form_view();
            }
            else
            {
                //lblError.Text = "Cant delete the record as it has dependencies.";
                //lblError.Visible = true;
                CommonTasks.WarningMessage("Cant delete the record as it has dependencies", Page);
            }
        }

    }
    protected void btnClear_Click(object sender, EventArgs e)
    {
        clear_form_view();
    }
}