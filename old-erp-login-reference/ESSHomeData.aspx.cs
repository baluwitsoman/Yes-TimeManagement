using AppCode;
using CommonObjects;
using DataAccess;
using ERP;
using ERP.AMM;
using ERP.ESS1.Workflow;
using ERP.HR;
using ERP.TAS;
using ERP1;
using ESS.Workflow;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Configuration;
using DataAccess.DA.ESS;
using System.Text;
using Newtonsoft.Json.Linq;
using DataAccess.Entities;
using ERP.EMail;
using DataAccess.DA.AMM;
using DataAccess.DA.Workflow;
using DataAccess.DA.Appointment;
using DataAccess.DA.ESS.AllServicesDA;
using DataAccess.DA.HR;
using DataAccess.DA.ESS.MyDetail;
using DataAccess.Dapper;
using Wits_Libraries;

namespace ERPWebApp.ESS_AER
{
    public partial class ESSHomeData : System.Web.UI.Page
    {
        DBAccess db = new DBAccess();
        public string currentempCode = "";
        public UserDatail user = null;
        public string InTimeDisp = "";
        EssLeaveRequestHeader essLeaveRequestHeader { get { return essLeaveRequestDA.leaveReqHeader; } }
        EssLeaveRequestDA essLeaveRequestDA = null;
        protected override void InitializeCulture()
        {
            if (Session["userDetail"] != null)
            {
                string strUserCulture = (Session["userDetail"] as UserDatail).UserCulture;


                //System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.CreateSpecificCulture(strUserCulture);
                System.Threading.Thread.CurrentThread.CurrentUICulture =
                    new System.Globalization.CultureInfo(strUserCulture);
                base.InitializeCulture();
            }
            else
                base.InitializeCulture();


        }
        private DataTable intialize_grid_doc()
        {

            var dt_details = new DataTable("dt_details");

            dt_details.Columns.Add("RowId", typeof(int));

            dt_details.Columns.Add("PEDD_DOC_SRL_NO");
            dt_details.Columns.Add("PEDD_DEPENDENT_CODE");
            dt_details.Columns.Add("PEDD_EMP_DOCUMENT_CODE");
            dt_details.Columns.Add("PEDD_EMP_DOCUMENT_REFERENCE");
            dt_details.Columns.Add("PEDD_EMP_DOCUMENT_ISSUE_PLACE");
            dt_details.Columns.Add("PEDD_EMP_DOCUMENT_ISSUE_DATE");
            dt_details.Columns.Add("PEDD_EMP_DOCUMENT_EXPIRY_DATE");
            dt_details.Columns.Add("PEDD_EMP_DOCUMENT_REMARKS");
            dt_details.Columns.Add("chk_delete");
            dt_details.Columns.Add("DEPENDENT_NAME");
            dt_details.Columns.Add("PDCM_DOCUMENT_DESC");


            return dt_details;
        }

  
        void LOAD_BUDGET_APPROVED_BYME()
        {
            MyRecruitmentBudgerDA myRecruitmentBudgerDA = new MyRecruitmentBudgerDA();
            myRecruitmentBudgerDA.LOAD_BUDGET_APPROVED_BYME(Request["EmpCode"]);
          
        }

        void LOAD_MY_REC_BUDGET()
        {
            MyRecruitmentBudgerDA myRecruitmentBudgerDA = new MyRecruitmentBudgerDA();
            myRecruitmentBudgerDA.LOAD_MY_REC_BUDGET(Request["EmpCode"]);
        }
        void SwitchLang()
        {
            UserSessionManagementDA userSessionManagementDA = new UserSessionManagementDA(user);
            userSessionManagementDA.SwitchLang();
        }
        void REMOVE_APPROVER_GROUOP()
        {
            MoveWorkflowToAnyLevelDA moveWorkflowToAnyLevelDA = new MoveWorkflowToAnyLevelDA();
            moveWorkflowToAnyLevelDA.REMOVE_APPROVER_GROUOP();
        }

        void PROBATION_GO_BACK()
        {
            MoveWorkflowToAnyLevelDA mveWorkflowToAnyLevelDA = new MoveWorkflowToAnyLevelDA();
            var errr = "";
            var result = mveWorkflowToAnyLevelDA.MoveWorkflowToPreviousLevel(Request["ReqId"].ToLong(), Request["approveComment"], out errr);
            if (result)
            {
                WriteSuccessMessage("SUccessfully moved to previous level.");

            }
            else
            {
                WriteErrorMessage(errr);

            }
        }
        void SAVE_LOCATION_HIERARCHY()
        {

        }

        void APPOINTMENT_LINK()
        {
            AppointmentLinkDA appointmentDA = new AppointmentLinkDA();
            appointmentDA.APPOINTMENT_LINK();
        }
        private readonly Dictionary<string, Action> _methodHandlers = new Dictionary<string, Action>();
        private void RegisterHandlers()
        {
            _methodHandlers["GET_ROLES"] = GET_ROLES1;
            _methodHandlers["MOVE_WORKFLOW_TO_THIS_LEVEL"] = MOVE_WORKFLOW_TO_THIS_LEVEL;
            _methodHandlers["APPOINTMENT_LINK"] = APPOINTMENT_LINK;
            _methodHandlers["SAVE_LOCATION_HIERARCHY"] = SAVE_LOCATION_HIERARCHY;
            _methodHandlers["APPROVER_CHANGE"] = APPROVER_CHANGE;
            _methodHandlers["REMOVE_APPROVER_GROUOP"] = REMOVE_APPROVER_GROUOP;
            _methodHandlers["SWITCHLANG"] = SwitchLang;
            _methodHandlers["LOAD_MY_COMPOFF"] = LOAD_MY_COMPOFF;
            _methodHandlers["LOADCOMPOFFHISTORYAPPROVEDBYME"] = LOADCOMPOFFHISTORYAPPROVEDBYME;
            _methodHandlers["LOAD_MY_REC_BUDGET"] = LOAD_MY_REC_BUDGET;
            _methodHandlers["LOAD_BUDGET_APPROVED_BYME"] = LOAD_BUDGET_APPROVED_BYME;
            _methodHandlers["GET_DEPENDANTS"] = GET_DEPENDANTS;
            _methodHandlers["LOAD_DEPNDANTS"] = LOAD_DEPNDANTS;
            _methodHandlers["LOAD_TRAININGS"] = LOAD_TRAININGS;
            _methodHandlers["DOC_UPLOAD"] = DOC_UPLOAD;
            _methodHandlers["LOAD_HISTORY"] = LOAD_HISTORY;
            _methodHandlers["GET_DOCUMENTS"] = GET_DOCUMENTS;
            _methodHandlers["HR_DOC_DELETE"] = HR_DOC_DELETE;
            _methodHandlers["SAVE_EMP_DOCS"] = SAVE_EMP_DOCS;
            _methodHandlers["LOAD_DEPENDANT_DOCS"] = LOAD_DEPENDANT_DOCS;
            _methodHandlers["GET_ANN_FILES"] = GET_ANN_FILES;
            _methodHandlers["LOADDOCSMASTERS"] = LoadDOcsMasters;
            _methodHandlers["GETDOCTORDETAILSUGGESTION"] = GetDoctorName;
            _methodHandlers["GET_SUGGESTION_FOR_HOSPITAL"] = GET_SUGGESTION_FOR_HOSPITAL;
            _methodHandlers["PROBATION_GO_BACK"] = PROBATION_GO_BACK;
 
            // Add more
        }
        private void GET_ROLES1()
        {
            var service = new AmmRolesDA();
            service.user = user;
            service.GET_ROLES();
        }

        protected void Page_Load(object sender, EventArgs e)
        {

            
            Response.Clear();
            Response.ContentType = "application/json";
            LoadUser();

            RegisterHandlers();
            Action handler;

            var method = Request.QueryString["Method"];
            if (_methodHandlers.TryGetValue(method,    out handler))
            {
                handler();
                return;
            }




            if (Request.QueryString["Method"] == "SAVE_LOAN")
            {
                SaveLoan();
            }
            else if (Request.QueryString["Method"] == "LOAN_ELIGIBLE")
            {
                string param1 = "";
                LoanEligibility(out param1, "", DateTime.Now);

                Response.Write(param1);
            }
            else if (Request.QueryString["Method"] == "LOAD_MY_LOAN")
            {
                LOAD_MY_LOAN();
            }
            else if (Request.QueryString["Method"] == "LOAD_MY_LOANAPPROVED")
            {
                LOAD_MY_LOANAPPROVED();
            }
            else if (Request.QueryString["Method"] == "GET_EMP_ON_LEAVE_TODAY")
            {
                GET_EMP_ON_LEAVE_TODAY();
            }
            else if (Request.QueryString["Method"] == "EXIT_CANCELLATION_REQ")
            {
                EXITCANCELLATION_REQ();
            }
            else if (Request.QueryString["Method"] == "EXIT_CANCEL_HISTORY")
            {
                LOAD_EXIT_CANCEL_HISTORY(Request["EmpCode"]);
            }
            else if (Request.QueryString["Method"] == "APP_HISTORY_FOR_HR")
            {
                APP_HISTORY_FOR_HR();
            }

            else if (Request.QueryString["Method"] == "EXIT_CANCEL_APPROVED")
            {
                LOAD_EXIT_CANCEL_APPROVED(Request["EmpCode"]);
            }
            else if (Request.QueryString["Method"] == "EXITCANCEL_APP")
            {
                Exit_Cancel_Approve();
            }
            else if (Request.QueryString["Method"] == "EXITCANCEL_REJ")
            {
                RejectReq();
            }
            else if (Request.QueryString["Method"] == "SAVE_RESIGNATION")
            {
                SaveTerminationOrResignation();
            }
            else if (Request.QueryString["Method"] == "SAVE_TERMINATION")
            {
                SaveTermination();
            }
            else if (Request.QueryString["Method"] == "EXIT_FORM")
            {
                SaveExitFormn();
            }
            else if (Request.QueryString["Method"] == "GET_RESIGNATION_DATE")
            {
                GET_RESIGNATION_DATE();
            }

            else if (Request.QueryString["Method"] == "GET_REASON_FOR_RESIGNATION")
            {
                GET_REASON_FOR_RESIGNATION();
            }
            else if (Request.QueryString["Method"] == "PERMISSION_SESSION_CNT")
            {
                PERMISSION_SESSION_CNT();
            }
            else if (Request.QueryString["Method"] == "USER_ROLES")
            {
                USER_ROLES();
            }
            else if (Request.QueryString["Method"] == "SAVE_APPRDOC")
            {
                var reqid = Request.QueryString["ReqheadId"];
                GENERALApproverDocs("13", reqid);
                WriteSuccessMessage("Success");
            }
            else if (Request.QueryString["Method"] == "USER_ROLES_ONTC")
            {
                USER_ROLES_ONTC();
            }
            else if (Request.QueryString["Method"] == "AER_LOGIN2")
            {
                AER_LOGIN2();
            }

            else if (Request.QueryString["Method"] == "CHECK_DATE")
            {
                CHECK_DATE();
            }
            else if (Request.QueryString["Method"] == "AER_LOGIN")
            {
                AER_LOGIN();
            }
            else if (Request.QueryString["Method"] == "GetMyApproval")
            {
                GetMyApproval();
            }
            else if (Request.QueryString["Method"] == "LoadmyRequest")
            {
                LoadmyRequest();
            }
            else if (Request.QueryString["Method"] == "EmployeeOfMonth")
            {
                LoadEmployeeOfMonthDetails();
            }
            else if (Request.QueryString["Method"] == "CheckEmpDet")
            {
                GetCheckEmp();
            }
            else if (Request.QueryString["Method"] == "GetRequestTypes")
            {
                GetRequestTypes();
            }
            else if (Request.QueryString["Method"] == "GetRequestLeaveTypes")
            {
                GetRequestLeaveTypeOnly();
            }
            else if (Request.QueryString["Method"] == "Forgotpassword")
            {
                SendForgotMail();
            }
            else if (Request.QueryString["Method"] == "APPRAISAL")
            {
                //SaveDocs("1");
                var otherempCode = "";
                if (!string.IsNullOrEmpty(Request["empToApplyLeaveHidden"]))
                {
                    otherempCode = Request["empToApplyLeaveHidden"];
                }
                else
                    otherempCode = user.Emp_Code;

                //appraisaltype                 
                var appraisalytype = Request["ddlapraisaltype"];
                var finalscore = Request["txtfinalscore"];
                var Desc = Request["leavedesc"];
                var empcode = user.Emp_Code;
                essLeaveRequestDA = new EssLeaveRequestDA();
                essLeaveRequestHeader.LeaveReqDetails.Add(new EssLeaveRequest
                {
                    ELR_LEAVE_REQ_ID = (essLeaveRequestHeader.LeaveReqDetails.Count() > 0 ? essLeaveRequestHeader.LeaveReqDetails.Max(m => m.ELR_LEAVE_REQ_ID) + 1 : 1),
                    ELR_SERVICE_TYPE = ESSServices.Appraisal
                });

                EssLeaveRequest ess = essLeaveRequestHeader.LeaveReqDetails[0];
                ess.ELR_EMP_ID = otherempCode;
                ess.ELR_LEAVE_ID = appraisalytype;
                ess.ELR_BODY = ess.ELR_SUBJ = Desc;
                ess.ELR_SERVICE_TYPE = ESSServices.Appraisal;
                ess.ELR_FROM_DT = DateTime.Now;
                ess.ELR_TO_DT = DateTime.Now;
                ess.ELR_TOT_DAYS = finalscore.ToDouble();
                ess.ELR_APPLY_EMP_CODE = empcode;
                essLeaveRequestHeader.ELR_FROM_DT = essLeaveRequestHeader.ELR_TO_DT = ess.ELR_FROM_DT;
                essLeaveRequestHeader.ELR_LEAVE_ID = ess.ELR_LEAVE_ID;
                essLeaveRequestHeader.ELR_SUBJ = ess.ELR_SUBJ;
                essLeaveRequestHeader.ELR_BODY = ess.ELR_BODY;
                essLeaveRequestHeader.ELR_EMP_ID = ess.ELR_EMP_ID;
                essLeaveRequestHeader.ELR_TOT_DAYS = ess.ELR_TOT_DAYS;

                essLeaveRequestHeader.ELR_SERVICE_TYPE = ESSServices.Appraisal;

                essLeaveRequestHeader.ELR_APPLY_EMP_CODE = ess.ELR_APPLY_EMP_CODE;
                essLeaveRequestHeader.ELR_CREATED_USER_ID = Request.Form["UserId"];

                if (ess.ELR_SUBJ.Length > 499)
                {
                    WriteErrorMessage("Error: Total characters cant be more than 500.");
                    return;
                }


                if (db.execute_scalar("select ELR_EMP_ID from ess_leave_req where elr_service_type=35 and ELR_EMP_ID='" + ess.ELR_EMP_ID + "' AND ELR_REQ_STATUS_ID=3 ") != "")
                {
                    WriteErrorMessage("Error: Previous request is pending for Employee - " + ess.ELR_EMP_ID + "");
                    return;
                }

                SaveDocs("35", otherempCode);
                if (essLeaveRequestDA.Apply("", false, "01"))
                {
                    WriteSuccessMessage("The request is sent.");
                }
                else
                {
                    WriteErrorMessage("Error: Unable to send the request.");
                }

                DeleteFiles("35", otherempCode);

            }
            else if (Request.QueryString["Method"] == "LEAVE")
            {
                //SaveDocs("1");
                var empCode = "";
                if (!string.IsNullOrEmpty(Request["empToApplyLeaveHidden"]))
                {
                    empCode = Request["empToApplyLeaveHidden"];
                }
                else
                    empCode = user.Emp_Code;
                SaveLeave();

                DeleteFiles("1", empCode);

            }
            else if (Request.QueryString["Method"] == "LEAVEPLANNER")
            {

                var empCode = "";
                if (!string.IsNullOrEmpty(Request["empToApplyLeaveHidden"]))
                {
                    empCode = Request["empToApplyLeaveHidden"];
                }
                else
                    empCode = user.Emp_Code;
                SaveLeavePlanner();

                DeleteFiles("37", empCode);

            }

            else if (Request.QueryString["Method"] == "PENDING_DOC_UPLOAD")
            {
                var REQ_ID = Request.QueryString["Req_Id"];
                var EMP_CODE = Request.QueryString["EmpCode"];
                var SERVICE = Request.QueryString["Service"];
                // var dt = objDB.execute_query_retun_datatable(@"Select ELR_SERVICE_TYPE, ELR_EMP_ID from ESS_LEAVE_REQ WHERE ELR_LEAVE_REQ_HEAD_ID='"+ REQ_ID+"'");
                //SERVICE = dt.Rows[0][0].ToString(); EMP_CODE = dt.Rows[0][1].ToString();
                PendingDocs(EMP_CODE, REQ_ID, SERVICE);
                WriteSuccessMessage("File uploaded Successfully.");
            }

            else if (Request.QueryString["Method"] == "ANNUALLEAVE")
            {
                SaveDocs("26");
                SaveAnnualLeave();
                DeleteFiles("26");
            }
            else if (Request.QueryString["Method"] == "LEAVECANCEL")
            {
                SaveDocs("28");
                SaveLeaveCancel();
                DeleteFiles("28");
            }
            else if (Request.QueryString["Method"] == "PERMISSION")
            {
                SaveDocs("3");
                try { SavePermission(); }
                catch (Exception eee) {
                    WriteErrorMessage("Error: " + eee.ToString());
                }
                DeleteFiles("3");
            }
            else if (Request.QueryString["Method"] == "CHECKINOUT")
            {
                SaveDocs("32");
                SaveCheckinout();
                DeleteFiles("32");
            }
            else if (Request.QueryString["Method"] == "COMPOFF")
            {
                SaveDocs("19");
                SaveCompoff();
                DeleteFiles("19");
            }

            else if (Request.QueryString["Method"] == "LEAVE_CARRYFORWARD")
            {
                SaveDocs("15");
                SaveLeaveCarryward();
                DeleteFiles("15");
            }
            else if (Request.QueryString["Method"] == "ATT")
            {
                SaveDocs("5");
                SaveAtt();
                DeleteFiles("5");

            }
            else if (Request.QueryString["Method"] == "TECH")
            {
                SaveDocs("7");
                SaveTECH();
                DeleteFiles("7");

            }
            else if (Request.QueryString["Method"] == "LETTER_REQ")
            {
                SaveDocs("13");
                LETTER_REQ();
                DeleteFiles("13");

            }
			else if (Request.QueryString["Method"] == "CHANGE_BANK_ACCOUNT")
			{
				SaveDocs("44");
				CHANGE_BANK_ACCOUNT_REQ();
				DeleteFiles("44");

			}
			else if (Request.QueryString["Method"] == "CHANGE_BANK_HISTORY")
            {
                PopulateChangeSalHist();
             }
            else if (Request.QueryString["Method"] == "CHANGE_BANK_APPROVED")
            {
                PopulateChangeSalApproved();
            }

            else if (Request.QueryString["Method"] == "REQ_APP_AL")
            {
                ANNUALLEAVE_Approve();
            }
            else if (Request.QueryString["Method"] == "REQ_APP_CHECK")
            {
                CHECKINOUT_Approve();
            }
            else if (Request.QueryString["Method"] == "REQ_APP")
            {
                ApproveReq();
            }
            else if (Request.QueryString["Method"] == "REQ_LEAVE_CANCEL")
            {
                ApproveLeaveCancel();
            }
            else if (Request.QueryString["Method"] == "PROB_APPRAISAL")
            {
                var Services = Request.QueryString["Service"];
                SaveDocAppraisal(Services);
                Appraisal();

            }
            else if (Request.QueryString["Method"] == "PROB_APPRAISAL_2")
            {
                var Services = Request.QueryString["Service"];
                SaveDocAppraisal(Services);
                Appraisal2();

            }
            else if (Request.QueryString["Method"] == "ENCASHLEAVE_REQ")
            {
                GETENCASHLEAVE_REQ(Request["EmpCode"]);
            }
            else if (Request.QueryString["Method"] == "ENCASHMENT_REQ")
            {
                ENCASHMENT_REQ();
            }
            //done by bala
            else if (Request.QueryString["Method"] == "ENCASHMENTREQ_APP")
            {
                ENCASHMENT_Approve();
            }
            else if (Request.QueryString["Method"] == "REJ_ENCASHMENTREQ")
            {
                RejectReq();
            }
            else if (Request.QueryString["Method"] == "ENCASHMENT_HISTORY")
            {
                LOAD_ENCASHMENT_HISTORY(Request["EmpCode"]);
            }
            else if (Request.QueryString["Method"] == "ENCASHMENT_APPROVED")
            {
                LOAD_ENCASHMENT_APPROVED(Request["EmpCode"]);
            }
            else if (Request.QueryString["Method"] == "GENERAL_REQ")
            {
                GENERAL_REQ();
            }

            else if (Request.QueryString["Method"] == "GENERALREQ_APP")
            {
                GeneralRequestApprove();
            }
            else if (Request.QueryString["Method"] == "REJ_GENERALREQ")
            {
                RejectReq();
            }
            else if (Request.QueryString["Method"] == "GENERAL_HISTORY")
            {
                LOAD_GENERAL_HISTORY(Request["EmpCode"]);
            }
            else if (Request.QueryString["Method"] == "GENERAL_APPROVED")
            {
                LOAD_GENERAL_APPROVED(Request["EmpCode"]);
            }

            else if (Request.QueryString["Method"] == "REJ_APPRAISAL")
            {
                RejectAppraisal();
                //,,,,
            }
            else if (Request.QueryString["Method"] == "GETEMPLOYEEDETAILSMANGER")
            {
                GetEmployeeDetailsManger();
            }
            else if (Request.QueryString["Method"] == "APPROVEDBYMYSELF")
            {
                APPROVEDBYMYSELF(Request["EmpCode"]);
            }
            else if (Request.QueryString["Method"] == "VIEW_ATT_HISTORY")
            {
                ATTENDANCEVIEW(Request["EmpCode"]);
            }
            else if (Request.QueryString["Method"] == "VIEW_ATT_OTHERS_HISTORY")
            {
                ATTENDANCEVIEW_OTHERS(Request["EmpCode"]);
            }
            else if (Request.QueryString["Method"] == "VIEW_ATT_PUNCH")
            {
                ATTENDANCPUNCHEVIEW(Request["EmpCode"]);
            }
            else if (Request.QueryString["Method"] == "APP_HISTORY")
            {
                APPRAISALHISTORY(Request["EmpCode"]);
            }
            else if (Request.QueryString["Method"] == "APP_APPROVED")
            {
                APPRAISALAPPROVEDBYME(Request["EmpCode"]);
            }
            else if (Request.QueryString["Method"] == "REQ_REJ")
            {
                RejectReq();
            }
            else if (Request.QueryString["Method"] == "APPROVER_DETAIL")
            {
                APPROVER_DETAIL();
            }
            else if (Request.QueryString["Method"] == "WF_STATUS")
            {
                WF_STATUS();
            }
            else if (Request.QueryString["Method"] == "PAYSLIP")
            {
                PAYSLIP();
            }

            else if (Request.QueryString["Method"] == "GetMangerDetails")
            {
                GetMangerDetails();
            }
            else if (Request.QueryString["Method"] == "EMP_DET_SAVE")
            {
                EMP_DET_SAVE();
            }
            else if (Request.QueryString["Method"] == "DESIGNATION")
            {
                DESIGNATION();
            }
            else if (Request.QueryString["Method"] == "DIVISION")
            {
                DIVISION();
            }
            else if (Request.QueryString["Method"] == "GRADE")
            {
                GRADE();
            }
            else if (Request.QueryString["Method"] == "SUBGRADE")
            {
                SUBGRADE();
            }
            else if (Request.QueryString["Method"] == "REPORTING_MANAGER")
            {
                REPORTING_MANAGER();
            }
            else if (Request.QueryString["Method"] == "SECTOR")
            {
                SECTOR();
            }

            else if (Request.QueryString["Method"] == "DEPENDANT_SAVE")
            {
                DEPENDANT_SAVE();
            }
            else if (Request.QueryString["Method"] == "DEPENDANT_DELETE")
            {
                DEPENDANT_DELETE();
            }
            else if (Request.QueryString["Method"] == "EDU_SAVE")
            {
                EDU_SAVE();
            }
            else if (Request.QueryString["Method"] == "EDU_DELETE")
            {
                EDU_DELETE();
            }
            else if (Request.QueryString["Method"] == "TRAINING_SAVE")
            {
                TRAINING_SAVE();
            }
            else if (Request.QueryString["Method"] == "TRAINING_DELETE")
            {
                TRAINING_DELETE();
            }
            else if (Request.QueryString["Method"] == "HISTORY_SAVE")
            {
                HISTORY_SAVE();
            }
            else if (Request.QueryString["Method"] == "HISTORY_DELETE")
            {
                HISTORY_DELETE();
            }
            else if (Request.QueryString["Method"] == "WARNING_VIEW")
            {
                WARNING_VIEW();
            }
            else if (Request.QueryString["Method"] == "QUAL_DOC_UPLOAD")
            {
                QUAL_DOC_UPLOAD();
            }
            else if (Request.QueryString["Method"] == "TRAINING_DOC_UPLOAD")
            {
                TRAINING_DOC_UPLOAD();


            }
            else if (Request.QueryString["Method"] == "GET_QUAL_DOCUMENTS")
            {
                GET_QUAL_DOCUMENTS();
            }
            else if (Request.QueryString["Method"] == "GET_TRAINING_DOCUMENTS")
            {
                GET_TRAINING_DOCUMENTS();
            }
            else if (Request.QueryString["Method"] == "FILE_DELETE")
            {
                FILE_DELETE();
            }
            else if (Request.QueryString["Method"] == "TRAININGFILE_DELETE")
            {
                TRAININGFILE_DELETE();
            }
            else if (Request.QueryString["Method"] == "GET_EDU_DETAIL")
            {
                GetEduDetail();
            }
            else if (Request.QueryString["Method"] == "REQ_DETAIL")
            {
                REQ_DETAIL();
            }
            else if (Request.QueryString["Method"] == "CANCEL_REQUEST")
            {
                CANCEL_REQUEST();
            }

            else if (Request.QueryString["Method"] == "LEAVE_BALANCE")
            {
                LEAVE_BALANCE(Request["EmpCode"], Request["LeaveId"]);
            }
            else if (Request.QueryString["Method"] == "LEAVE_HISTORY")
            {
                LEAVE_HISTORY(Request["EmpCode"], Request["LeaveId"]);
            }
            else if (Request.QueryString["Method"] == "LEAVE_HISTORY_PLANNER")
            {
                LEAVE_HISTORY_PLANNER(Request["EmpCode"], Request["LeaveId"]);
            }
            else if (Request.QueryString["Method"] == "LEAVE_CANCELHISTORY")
            {
                LEAVECANCEL_HISTORY(Request["EmpCode"], Request["LeaveId"]);
            }
            else if (Request.QueryString["Method"] == "ANN_LEAVE_HISTORY")
            {
                ANNUALLEAVE_HISTORY(Request["EmpCode"], Request["LeaveId"]);
            }
            else if (Request.QueryString["Method"] == "PERMISSION_HISTORY")
            {
                PERMISSION_HISTORY(Request["EmpCode"], Request["LeaveId"]);
            }
            else if (Request.QueryString["Method"] == "GET_SALARY_BANK")
            {
                GET_SALARY_BANK(Request["EmpCode"]);
            }

            else if (Request.QueryString["Method"] == "LOADLEAVEHISTORYAPPROVEDBYME")
            {
                LOADLEAVEHISTORYAPPROVEDBYME(Request["EmpCode"]);
            }
            else if (Request.QueryString["Method"] == "GETCHECK_TIME_BALANCE")
            {
                LOADGETCHECK_TIME_BALANCE(Request["EmpCode"]);
            }
            else if (Request.QueryString["Method"] == "LOAD_LEAVE_APPLIED_BY_ME")
            {
                LOAD_LEAVE_APPLIED_BY_ME(Request["EmpCode"]);
            }

            else if (Request.QueryString["Method"] == "GetAllEmployees")
            {
                LoadGetAllEmployees(Request["isactive"]);
            }
            else if (Request.QueryString["Method"] == "Getlinemanger")
            {
                LoadGetlinemanger(Request["empid"]);
            }
            else if (Request.QueryString["Method"] == "GetAssetDetails")
            {
                LoadGetAssetDetails(Request["empid"]);
            }
            else if (Request.QueryString["Method"] == "GetActingmanger")
            {
                LoadGetActingmanger(Request["empid"]);
            }
            else if (Request.QueryString["Method"] == "LOADLEAVEBALANCEBYME")
            {
                LOADLEAVEBALACEAPPROVEDBYME(Request["EmpCode"]);
            }
            else if (Request.QueryString["Method"] == "LOADLEAVEBALANCELEAVEPLAN")
            {
                LOADLEAVEBALANCELEAVEPLANNER(Request["EmpCode"]);
            }
            else if (Request.QueryString["Method"] == "CARRYFORWARD_APPROVED_BY_ME")
            {
                CARRYFORWARD_APPROVED_BY_ME(Request["EmpCode"]);
            }
            else if (Request.QueryString["Method"] == "LETTER_APPROVED_BY_ME")
            {
                LOAD_APPROVED_LETTER_HISTORY_BY_ME(Request["EmpCode"]);
            }
            else if (Request.QueryString["Method"] == "LOAD_MYAPPRAISAL_HISTORY")
            {
                LOAD_MYAPPRAISAL_HISTORY(Request["EmpCode"]);
            }

            else if (Request.QueryString["Method"] == "LOAD_APPRAISAL_APPLIED_BY_ME")
            {
                LOAD_MYAPPRAISAL_APPLIED_BY_ME(Request["EmpCode"]);
            }
            else if (Request.QueryString["Method"] == "LOAD_MY_APPRAISAL")
            {
                LOAD_MY_APPRAISAL(Request["EmpCode"]);
            }
            else if (Request.QueryString["Method"] == "LOADMYAPPRAISALHISTORYAPPROVEDBYME")
            {
                LOADMYAPPRAISALHISTORYAPPROVEDBYME(Request["EmpCode"]);
            }
            else if (Request.QueryString["Method"] == "LOAD_MY_LEAVEPLANNER")
            {
                LOAD_MY_LEAVEPLANNER(Request["EmpCode"]);
            }
            else if (Request.QueryString["Method"] == "LOAD_MYLEAVEPLANNER_APPROVEDBYME")
            {
                LOAD_MY_LEAVEPLANNER_APPROVEDBYME(Request["EmpCode"]);
            }
            else if (Request.QueryString["Method"] == "LOAD_MY_LEAVES")
            {
                LOAD_MY_LEAVES(Request["EmpCode"]);
            }
            else if (Request.QueryString["Method"] == "LOAD_MY_APPOINTMENT")
            {
                LOAD_MY_APPOINTMENT_HISTORY(Request["EmpCode"]);
            }
            else if (Request.QueryString["Method"] == "LOAD_MY_APPOINTMENT_VIEW")
            {
                LOAD_MY_APPOINTMENT_APPROVED_VIEW(Request["EmpCode"]);
            }
            else if (Request.QueryString["Method"] == "LOAD_MY_ANNUAL")
            {
                LOAD_MY_ANNUALHISTORY(Request["EmpCode"]);
            }
            else if (Request.QueryString["Method"] == "LOAD_MY_ANNUALAPPROVAL")
            {
                LOADANNUALAPPROVEDBYME(Request["EmpCode"]);
            }
            else if (Request.QueryString["Method"] == "LOAD_MY_PERMISSION")
            {
                LOAD_MY_PERMISSIONHISTORY(Request["EmpCode"]);
            }
            else if (Request.QueryString["Method"] == "LOAD_MY_PERMISSIONAPPROVAL")
            {
                LOADPERMISSIONAPPROVEDBYME(Request["EmpCode"]);
            }
            else if (Request.QueryString["Method"] == "LOAD_MY_CHECKINHISTORY")
            {
                LOAD_MY_CHECKINHISTORY(Request["EmpCode"]);
            }
            else if (Request.QueryString["Method"] == "LOAD_MY_CHECKINAPPROVED")
            {
                LOADCHECKINAPPROVEDBYME(Request["EmpCode"]);
            }
            else if (Request.QueryString["Method"] == "LOAD_MY_GETLEAVEDETAILS")
            {
                LOAD_MY_GETLEAVEDETAILS(Request["Reqid"]);
            }
            else if (Request.QueryString["Method"] == "LOAD_MY_LEAVEDETAILS")
            {
                LOAD_MY_LEAVEDETAILS(Request["EmpCode"]);
            }
            else if (Request.QueryString["Method"] == "LOAD_MY_CANCEL_HISTORY")
            {
                LOAD_MY_CANCEL_HISTORY(Request["EmpCode"]);
            }
            else if (Request.QueryString["Method"] == "LOAD_MY_CANCEL_APPROVED")
            {
                LOAD_MY_CANCEL_HISTORYAPPROVEDBYME(Request["EmpCode"]);
            }
            else if (Request.QueryString["Method"] == "LOAD_CHECKBAL")
            {
                LOAD_MY_LEAVECHECKBAL();
            }
            else if (Request.QueryString["Method"] == "LOAD_LEAVECHECK")
            {
                // changed by balu for incorporating Applyforothers
                LOAD_VALIDATELEAVE(Request.Form["empToApplyLeaveHidden"]);

            }
            else if (Request.QueryString["Method"] == "LOAD_MY_LEAVE_CARRYFOWARD")
            {
                LOAD_MY_LEAVE_CARRYFOWARD(Request["EmpCode"]);
            }
            else if (Request.QueryString["Method"] == "GETEMPLOYEEDETAILSALL")
            {
                GetEmployeeDetailSAll();
            }
            else if (Request.QueryString["Method"] == "GETEMPLOYEEDETAILSUGGESTION")
            {
                GetEmployeeDetailSuggestion();
            }
            else if (Request.QueryString["Method"] == "GETEMPLOYEEDETAILSUGGESTIONWITHOUTBRANCH")
            {
                GetEmployeeDetailSuggestionWithoutBranch();
            }

            else if (Request.QueryString["Method"] == "GETEMPLOYEEDETAILSACTIVE")
            {
                GetEmployeeDetailsActive();
            }

            else if (Request.QueryString["Method"] == "GETEMPLOYEEDETAILS")
            {
                GetEmployeeDetails(Request["EmpCode"]);
            }
            else if (Request.QueryString["Method"] == "ESS_WF_EMP_MAP")
            {
                //Request URL: http://localhost:57813/ess-aer/ESSHomeData.aspx?Method=ESS_WF_EMP_MAP&EmpCode=062&EssId=100

                ESS_WF_EMP_MAP();
            }

            else if (Request.QueryString["Method"] == "ESS_WF_EMP_MAP_SAVE")
            {
                ESS_WF_EMP_MAP_SAVE();
            }

            else if (Request.QueryString["Method"] == "GET_TRAVEL_EXP_DOCS")
            {
                GET_TRAVEL_EXP_DOCS();
            }
            else if (Request.QueryString["Method"] == "LEAVE_TYPE_ACTING_ENABLED_YN")
            {
                LEAVE_TYPE_ACTING_ENABLED_YN();
            }
            else if (Request.QueryString["Method"] == "OT_REQUEST")
            {
                LOAD_MY_OTDETAILS(Request["EmpCode"]);
            }
            else if (Request.QueryString["Method"] == "OT_REQUESTAPPROVED")
            {
                LOAD_MY_OTAPPROVED(Request["EmpCode"]);
            }
            else if (Request.QueryString["Method"] == "TimeSheet_Request")
            {
                LOAD_MY_TIMESHEETDETAILS(Request["EmpCode"]);
            }
            else if (Request.QueryString["Method"] == "TimeSheet_RequestAPPROVED")
            {
                LOAD_MY_TIMESHEETAPPROVED(Request["EmpCode"]);
            }
            else if (Request.QueryString["Method"] == "MDM_MATERIAL")
            {
                LOAD_MATERIAL();
            }
            else if (Request.QueryString["Method"] == "MDM_VENDOR")
            {
                LOAD_VENDOR();
            }
            else if (Request.QueryString["Method"] == "MDM_CUSTOMER")
            {
                LOAD_CUSTOMER();
            }
            else if (Request.QueryString["Method"] == "EDIT_CUSTOMER")
            {
                EDIT_CUSTOMER();
            }
            else if (Request.QueryString["Method"] == "Resignation")
            {
                Resignation(Request["EmpCode"]);
            }
            else if (Request.QueryString["Method"] == "Resignationapproved")
            {
                Resignationapproved(Request["EmpCode"]);
            }
            else if (Request.QueryString["Method"] == "Termination")
            {
                Termination(Request["EmpCode"]);
            }
            else if (Request.QueryString["Method"] == "Terminationapproved")
            {
                Terminationapproved(Request["EmpCode"]);
            }
            else if (Request.QueryString["Method"] == "ExitFormHistory")
            {
                ExitForm_History(Request["EmpCode"]);
            }
            else if (Request.QueryString["Method"] == "ExitFormApproved")
            {
                ExitForm_Approved(Request["EmpCode"]);
            }

            else if (Request.QueryString["Method"] == "Empdownloads")
            {
                Empdownloads(Request["EmpCode"]);
            }
            else if (Request.QueryString["Method"] == "ESS_WF_DELETE")
            {
                ESS_WF_DELETE();
            }
            else if (Request.QueryString["Method"] == "ESS_WF_GENERATE")
            {
                GENERATEWORKFLOW(Request["Req_id"]);
            }
            else if (Request.QueryString["Method"] == "ESS_WF_DELETE_LINEITEMS")
            {
                ESS_WF_DELETE_LINEITEMS();
            }
            else if (Request.QueryString["Method"] == "INV_WF_DELETE")
            {
                INV_WF_DELETE();
            }

            else if (Request.QueryString["Method"] == "DOWNLOAD_VIDEOS")
            {
                DOWNLOAD_VIDEOS();
            }
            else if (Request.QueryString["Method"] == "ATT_HISTORY")
            {
                PopulateAttHist();
            }
            else if (Request.QueryString["Method"] == "ATT_APPROVED")
            {
                PopulateAttapproved();
            }
            else if (Request.QueryString["Method"] == "TECH_HISTORY")
            {
                PopulateTechhist();
            }
            else if (Request.QueryString["Method"] == "TECH_APPROVED")
            {
                PopulateTechapproved();
            }
            else if (Request.QueryString["Method"] == "PRINT_LETTER_UPDATE")
            {
                // PrintLetterUpdate();
            }
            else if (Request.QueryString["Method"] == "ADMIN_HISTORY")
            {
                Populateadminhist();
            }
            else if (Request.QueryString["Method"] == "ADMIN_APPROVED")
            {
                Populateadminapproved();
            }
            else if (Request.QueryString["Method"] == "LETTER_HISTORY")
            {
                PopulateLetterhist();
            }
            else if (Request.QueryString["Method"] == "TRAVEL_HISTORY")
            {
                PopulateTravelhist();
            }
            else if (Request.QueryString["Method"] == "TRAVEL_APPROVED")
            {
                PopulateTravelapproved();
            }
            else if (Request.QueryString["Method"] == "LOADManagerDetails")
            {
                LOAD_MY_ManagerDetails(Request["EmpCode"]);
            }
            else if (Request.QueryString["Method"] == "Get_Doc_Probation")
            {
                GET_PROBATION_DOCUMENTS();
            }
            else
            {
                ResponseWrite("");
            }
        }

        void GET_PROBATION_DOCUMENTS()
        {


            var servicetype = Request.QueryString["ServiceType"];
            var LeaveReqId = Request["LeaveReqId"];
            var empcode = Request["Empcode"];

            var dt = new DataTable();

            dt.Columns.Add("SrlNo");
            dt.Columns.Add("FileName");
            dt.Columns.Add("RelativePath");
            //dt.Columns.Add("DeletePath");

            if (!Directory.Exists(Server.MapPath("~/ESS/Docs/" + empcode + "/" + servicetype + "/" + LeaveReqId)))
            {
                return;
            }

            var files = Directory.GetFiles(Server.MapPath("~/ESS/Docs/" + empcode + "/" + servicetype + "/" + LeaveReqId));

            for (int i = 0; i < files.Length; i++)
            {

                var fileInf = new FileInfo(files[i]);

                var row = dt.NewRow();

                row["SrlNo"] = i + 1;
                row["FileName"] = fileInf.Name;
                row["RelativePath"] = ResolveUrl("~/ESS/Docs/" + empcode + "/" + servicetype + "/" + LeaveReqId) + "/" + fileInf.Name;
                //row["DeletePath"] = ("~/ESS/Docs/" + empCode + "/" + srlNo) + "/" + fileInf.Name;
                dt.Rows.Add(row);
            }


            var JSONresult = JsonConvert.SerializeObject(dt);
            Response.Write(JSONresult);
            //return JSONresult;
        }
        void GET_DOCUMENTS()
        {


            EssHelperMethods.GET_DOCUMENTS();
        }
        void LOAD_DEPNDANTS()
        {

            //EmployeeDependantsDA employeeDependantsDA = new EmployeeDependantsDA(user);
            //employeeDependantsDA.LOAD_DEPNDANTS();


          //  using (DapperDA dapper = new DapperDA())
            {


                string str_select_empear_ded_detail = @"
    select * from (
        SELECT 
            PDPD_DEPENDENT_CODE,
            PDPD_DEPENDENT_TITLE, 
            PDPD_DEPENDENT_NAME, 
            PDPD_DEPENDENT_SEX,
            PDPD_DEPENDENT_RELATION,
            (SELECT MCMD_ENTITY_DESC from mmm_common_masters_detail where MCMD_ENTITY_GROUP = 'RELATION' AND  MCMD_ENTITY_CODE = PDPD_DEPENDENT_RELATION) AS RELATION_DESCRIPTION,
            TO_CHAR(PDPD_DEPENDENT_DATE_BIRTH, 'DD/MM/YYYY')  AS PDPD_DEPENDENT_DATE_BIRTH , 
            TO_CHAR(PDPD_DEPENDENT_DATE_EXPIRY, 'DD/MM/YYYY')  AS  PDPD_DEPENDENT_DATE_EXPIRY,
            PDPD_DEPENDENT_COMP_INSURED,
            PDPD_DEPENDENT_AIRFARE_YN,
            PDPD_DEPENDENT_VISA_ISSUES_YN ,
            PDPD_REMARKS, 
            'N' as chk_delete,
            PDPD_DEPENDENT_TYPE,PDPD_LIB_PERC,PDPD_GSM,
                (  select sum(PDPD_LIB_PERC)     from 
                    PPT_EMP_DEPENDENT_DETAILS
                    WHERE PDPD_EMP_CODE =:Emp_Code) TOTAL
        from 
        PPT_EMP_DEPENDENT_DETAILS
        WHERE PDPD_EMP_CODE = :Emp_Code  )";
                DBAccess db = new DBAccess();
                var dt = db.execute_query_retun_datatable(str_select_empear_ded_detail,
                    new Dictionary<string, object> {
                        {"Emp_Code",user.Emp_Code }
                    }
                    );

                var JSONresult = JsonConvert.SerializeObject(dt);
                HttpContext.Current.Response.Write(JSONresult);
            }

        }

        void LOAD_TRAININGS()
        {

            EmployeeTrainingDA employeeTrainingDA = new EmployeeTrainingDA(user);
            employeeTrainingDA.LOAD_TRAININGS();
        }


        void LOAD_HISTORY()
        {

            EmployementHistoryDA employementHistoryDA = new EmployementHistoryDA(user);
            employementHistoryDA.LOAD_HISTORY();
        }
        void DOC_UPLOAD()
        {
            EssHelperMethods.DOC_UPLOAD();
        }
        void HR_DOC_DELETE()
        {
            HREmployeeDocumentDA hREmployeeDocumentDA = new HREmployeeDocumentDA(user);
            hREmployeeDocumentDA.DeleteEmpHRDoc();
        }
        void SAVE_EMP_DOCS()
        {
            EssDocumentSaveDA essDocumentSaveDA = new EssDocumentSaveDA();
            essDocumentSaveDA.SAVE_EMP_DOCS();


        }
        //var url1 = "ESSHomeData.aspx?Method=LOAD_DEPENDANT_DOCS&EmpCode=" + $('#EMPCode').val() + "&DepCode=" + depCode;
        void LOAD_DEPENDANT_DOCS()
        {
            EmployeeDependantsDA employeeDependantsDA = new EmployeeDependantsDA(user);
            employeeDependantsDA.LOAD_DEPENDANT_DOCS();

        }
        void LoadDOcsMasters()
        {

            DocumentMasterDA documentMasterDA = new DocumentMasterDA();
            documentMasterDA.LoadDOcsMasters();

        }
        void GET_DEPENDANTS()
        {
            EmployeeDependantsDA employeeDependantsDA = new EmployeeDependantsDA(user);
            employeeDependantsDA.GET_DEPENDANTS();

        }

        //SELECT PDPD_DEPENDENT_CODE  FROM ERP.PPT_EMP_DEPENDENT_DETAILS; 

        void Resignation(string empcode)
        {
            var fromDate1 = Request["FromDate"].ToArabicDate();
            var toDate1 = Request["ToDate"].ToArabicDate();
            DBAccess ObjDb = new DBAccess();
            //            string strquery = @"SELECT HTH_ROW_ID ROW_ID,HTH_TYPE_OF_TERMINATION TERMINATION,TO_CHAR(HTH_DATE_OF_RESIGANTION,'dd/MM/yyyy') DATE_OF_RESIGANTION,HTH_REASON_FOR_TERMINATION REASON,
            //TO_CHAR(HTH_DOCUMENT_DATE, 'dd/MM/yyyy') DOCUMENT_DATE FROM HRT_TERMINATION_HEADER  ";

            string strquery = @" SELECT
      ELR_EMP_ID, ELR_SUBJ, ELR_BODY,TO_CHAR(ELR_REQUESTED_DT, 'dd/MM/yyyy')   
      ELR_REQUESTED_DT, ELR_REQ_STATUS_ID, ELR_SERVICE_TYPE,ELR_LEAVE_REQ_HEAD_ID,TO_CHAR(ELR_FROM_DT, 'dd/MM/yyyy') ELR_FROM_DT,
      HTH_DOCUMENT_NO, HTH_DOCUMENT_DATE, case when HTH_TYPE_OF_TERMINATION = 'V' then 'Voluntary' ELSE 'Involuntary' END HTH_TYPE_OF_TERMINATION,
       HTH_REASON_FOR_TERMINATION,TO_CHAR(HTH_DATE_OF_RESIGANTION, 'dd/MM/yyyy')   HTH_DATE_OF_RESIGANTION, HTH_LAST_WORKING_DATE,
       HTH_COMMENTS, EMPID, ENAME, BRANCH_NAME, DEPT_NAME, DIV_NAME,CASE WHEN ELR_REQ_STATUS_ID ='11' THEN 'CANCELLED' ELSE APPROVED_GROUP_NAME END APPROVED_GROUP_NAME,
    APP_CNT ,TO_CHAR (ELR_APPROVED_DT, 'DD-MM-YYY hh24:mi') APP_DATE,
            TO_CHAR (ELR_APPROVED_DT, 'hh24:mi')APP_TIME
FROM
    ESS_LEAVE_REQ, HRT_TERMINATION_HEADER, VU_EMP_DET  ,
    ESS_REQ_APPROVED_GROUP,
         ( select AWDD_WF_HEADER_ROW_ID WF_HEADER_ID, SUM(CASE WHEN wd2.AWDD_STATUS in ('A','R') THEN 1 ELSE 0 END) AS APP_CNT
        from AUTH_WF_DOC_DETAIL wd2  
                    GROUP BY AWDD_WF_HEADER_ROW_ID)

            WHERE ELR_FROM_DT BETWEEN to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy')
        AND UNIQUE_ID_1 =  TO_CHAR(ELR_LEAVE_REQ_HEAD_ID)
        AND UNIQUE_ID_2 = TO_CHAR(ELR_LEAVE_REQ_HEAD_ID) 
        and WF_HEADER_ID(+) = AWH_ROW_ID1";

            strquery += @" AND ELR_SERVICE_TYPE = 12 and ELR_REF_DOC_NO = HTH_DOCUMENT_NO(+)    
            AND EMPID = ELR_EMP_ID AND EMPID = '" + empcode + @"'
 and ((ELR_FROM_DT BETWEEN to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') OR
        ELR_TO_DT BETWEEN to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy')) OR 
          ( ELR_FROM_DT < to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') < ELR_TO_DT ))
   

            order by ELR_LEAVE_REQ_HEAD_ID desc";
            DataTable dt = new DataTable();
            dt = ObjDb.execute_query_retun_datatable(strquery);
            if (dt.Rows.Count > 0)
            {
                var JSONresult = JsonConvert.SerializeObject(dt);
                Response.Write(JSONresult);
            }
            else
            {
                Response.Write("{}");
            }

        }

        void Resignationapproved(string empcode)
        {
            var fromDate1 = Request["FromDate"].ToArabicDate();
            var toDate1 = Request["ToDate"].ToArabicDate();

            DBAccess ObjDb = new DBAccess();
            //            string strquery = @"SELECT HTH_ROW_ID ROW_ID,PEMP_EMP_NAME EMPNAME,PBM_BRANCH_NAME BRANCH,HTH_TYPE_OF_TERMINATION TERMINATION,TO_CHAR(HTH_DATE_OF_RESIGANTION,'dd/MM/yyyy') DATE_OF_RESIGANTION,HTH_REASON_FOR_TERMINATION REASON,
            //TO_CHAR(HTH_DOCUMENT_DATE, 'dd/MM/yyyy') DOCUMENT_DATE FROM HRT_TERMINATION_HEADER , V_EMPLOYEE_DETAILS 
            //WHERE HTH_EMP_CODE = PEMP_EMP_CODE  ";

            string strquery = @" SELECT 
    ELR_EMP_ID, ELR_SUBJ, ELR_BODY, TO_CHAR(ELR_REQUESTED_DT, 'dd/MM/yyyy')
    ELR_REQUESTED_DT, ELR_REQ_STATUS_ID, ELR_SERVICE_TYPE,ELR_LEAVE_REQ_HEAD_ID,
    HTH_DOCUMENT_NO, HTH_DOCUMENT_DATE, case when HTH_TYPE_OF_TERMINATION ='V' then 'Voluntary' ELSE 'Involuntary' END HTH_TYPE_OF_TERMINATION, 
    HTH_REASON_FOR_TERMINATION,TO_CHAR(HTH_DATE_OF_RESIGANTION, 'dd/MM/yyyy') HTH_DATE_OF_RESIGANTION, HTH_LAST_WORKING_DATE, 
    HTH_COMMENTS   , EMPID, ENAME, BRANCH_NAME, DEPT_NAME, DIV_NAME,
    CASE WHEN ELR_REQ_STATUS_ID = 11  THEN          
            GET_LEAVE_STATUS (ELR_REQ_STATUS_ID)     
        ELSE  
            APPROVED_GROUP_NAME END APPROVED_GROUP_NAME  ,
    HTH_NOTICE_PERIOD_IN_DAYS,TO_CHAR (ELR_APPROVED_DT, 'DD-MM-YYY HH24:MI') APP_DATE,
            TO_CHAR (ELR_APPROVED_DT, 'hh24:mi')APP_TIME
FROM 
    ESS_LEAVE_REQ,HRT_TERMINATION_HEADER , VU_EMP_DET,
    ESS_REQ_APPROVED_GROUP 
            where HTH_DATE_OF_RESIGANTION BETWEEN to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy')
            and    ELR_SERVICE_TYPE = 12  
    and ELR_REF_DOC_NO = HTH_DOCUMENT_NO
    AND EMPID = ELR_EMP_ID AND EMPID = ELR_EMP_ID  
    AND UNIQUE_ID_1 = TO_CHAR(ELR_LEAVE_REQ_HEAD_ID)
    AND UNIQUE_ID_2 = TO_CHAR(ELR_LEAVE_REQ_HEAD_ID)
    and exists (
    SELECT 1 FROM AUTH_WF_HEADER, AUTH_WF_DOC_DETAIL
              WHERE
                    AWDD_WF_HEADER_ROW_ID = AWH_ROW_ID
   AND AWDD_EMP_APPROVED_BY = '" + empcode + @"' AND AWH_UNIQUE_ID1 = TO_CHAR(ELR_LEAVE_REQ_HEAD_ID) AND AWH_UNIQUE_ID2 = TO_CHAR(ELR_LEAVE_REQ_HEAD_ID))
    and((ELR_FROM_DT BETWEEN to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy') OR
        ELR_TO_DT BETWEEN to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy')) OR
            (ELR_FROM_DT < to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy') < ELR_TO_DT))
 order by ELR_LEAVE_REQ_HEAD_ID desc  ";


            DataTable dt = new DataTable();
            dt = ObjDb.execute_query_retun_datatable(strquery);
            if (dt.Rows.Count > 0)
            {
                var JSONresult = JsonConvert.SerializeObject(dt);
                Response.Write(JSONresult);
            }
            else
            {
                Response.Write("{}");
            }

        }


        void Termination(string empcode)
        {
            var fromDate1 = Request["FromDate"].ToArabicDate();
            var toDate1 = Request["ToDate"].ToArabicDate();
            DBAccess ObjDb = new DBAccess();
            //            string strquery = @"SELECT HTH_ROW_ID ROW_ID,HTH_TYPE_OF_TERMINATION TERMINATION,TO_CHAR(HTH_DATE_OF_RESIGANTION,'dd/MM/yyyy') DATE_OF_RESIGANTION,HTH_REASON_FOR_TERMINATION REASON,
            //TO_CHAR(HTH_DOCUMENT_DATE, 'dd/MM/yyyy') DOCUMENT_DATE FROM HRT_TERMINATION_HEADER  ";

            string strquery = @" SELECT
      ELR_EMP_ID, ELR_SUBJ, ELR_BODY,TO_CHAR(ELR_REQUESTED_DT, 'dd/MM/yyyy')   
      ELR_REQUESTED_DT, ELR_REQ_STATUS_ID, ELR_SERVICE_TYPE,ELR_LEAVE_REQ_HEAD_ID,
      HTH_DOCUMENT_NO, HTH_DOCUMENT_DATE, case when HTH_TYPE_OF_TERMINATION = 'V' then 'Voluntary' ELSE 'Involuntary' END HTH_TYPE_OF_TERMINATION,
       HTH_REASON_FOR_TERMINATION,TO_CHAR(HTH_DATE_OF_RESIGANTION, 'dd/MM/yyyy')   HTH_DATE_OF_RESIGANTION, HTH_LAST_WORKING_DATE,
       HTH_COMMENTS, EMPID, ENAME, BRANCH_NAME, DEPT_NAME, DIV_NAME,CASE WHEN ELR_REQ_STATUS_ID ='11' THEN 'CANCELLED' ELSE APPROVED_GROUP_NAME END APPROVED_GROUP_NAME,
    APP_CNT,TO_CHAR (ELR_APPROVED_DT, 'DD-MM-YYY hh24:mi') APP_DATE,
            TO_CHAR (ELR_APPROVED_DT, 'hh24:mi')APP_TIME 
FROM
    ESS_LEAVE_REQ, HRT_TERMINATION_HEADER, VU_EMP_DET  ,
    ESS_REQ_APPROVED_GROUP,
         ( select AWDD_WF_HEADER_ROW_ID WF_HEADER_ID, SUM(CASE WHEN wd2.AWDD_STATUS in ('A','R') THEN 1 ELSE 0 END) AS APP_CNT
        from AUTH_WF_DOC_DETAIL wd2  
                    GROUP BY AWDD_WF_HEADER_ROW_ID)

            WHERE HTH_DATE_OF_RESIGANTION BETWEEN to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy')
        AND UNIQUE_ID_1 =  TO_CHAR(ELR_LEAVE_REQ_HEAD_ID)
        AND UNIQUE_ID_2 = TO_CHAR(ELR_LEAVE_REQ_HEAD_ID) 
        and WF_HEADER_ID(+) = AWH_ROW_ID1";

            strquery += @" AND ELR_SERVICE_TYPE = 30 and ELR_REF_DOC_NO = HTH_DOCUMENT_NO    
            AND EMPID = ELR_EMP_ID AND ELR_CREATION_USER_ID = '" + user.User_Id + @"'
 and ((ELR_FROM_DT BETWEEN to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') OR
        ELR_TO_DT BETWEEN to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy')) OR 
          ( ELR_FROM_DT < to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') < ELR_TO_DT ))
   

            ORDER BY ELR_LEAVE_REQ_HEAD_ID DESC ";
            DataTable dt = new DataTable();
            log_error.write_to_log_file("Termination", "false", strquery);
            dt = ObjDb.execute_query_retun_datatable(strquery);
            if (dt.Rows.Count > 0)
            {
                var JSONresult = JsonConvert.SerializeObject(dt);
                Response.Write(JSONresult);
            }
            else
            {
                Response.Write("{}");
            }

        }

        void Terminationapproved(string empcode)
        {
            var fromDate1 = Request["FromDate"].ToArabicDate();
            var toDate1 = Request["ToDate"].ToArabicDate();

            DBAccess ObjDb = new DBAccess();
            //            string strquery = @"SELECT HTH_ROW_ID ROW_ID,PEMP_EMP_NAME EMPNAME,PBM_BRANCH_NAME BRANCH,HTH_TYPE_OF_TERMINATION TERMINATION,TO_CHAR(HTH_DATE_OF_RESIGANTION,'dd/MM/yyyy') DATE_OF_RESIGANTION,HTH_REASON_FOR_TERMINATION REASON,
            //TO_CHAR(HTH_DOCUMENT_DATE, 'dd/MM/yyyy') DOCUMENT_DATE FROM HRT_TERMINATION_HEADER , V_EMPLOYEE_DETAILS 
            //WHERE HTH_EMP_CODE = PEMP_EMP_CODE  ";

            string strquery = @" SELECT ELR_LEAVE_REQ_HEAD_ID,
    ELR_EMP_ID, ELR_SUBJ, ELR_BODY, TO_CHAR(ELR_REQUESTED_DT, 'dd/MM/yyyy')
    ELR_REQUESTED_DT, ELR_REQ_STATUS_ID, ELR_SERVICE_TYPE,ELR_LEAVE_REQ_HEAD_ID,
    HTH_DOCUMENT_NO, HTH_DOCUMENT_DATE, case when HTH_TYPE_OF_TERMINATION ='V' then 'Voluntary' ELSE 'Involuntary' END HTH_TYPE_OF_TERMINATION, 
    HTH_REASON_FOR_TERMINATION,TO_CHAR(HTH_DATE_OF_RESIGANTION, 'dd/MM/yyyy') HTH_DATE_OF_RESIGANTION, HTH_LAST_WORKING_DATE, 
    HTH_COMMENTS   , EMPID, ENAME, BRANCH_NAME, DEPT_NAME, DIV_NAME,
     APPROVED_GROUP_NAME  ,
    HTH_NOTICE_PERIOD_IN_DAYS,TO_CHAR (ELR_APPROVED_DT, 'DD-MM-YYY hh24:mi') APP_DATE,
            TO_CHAR (ELR_APPROVED_DT, 'hh24:mi')APP_TIME
FROM 
    ESS_LEAVE_REQ,HRT_TERMINATION_HEADER , VU_EMP_DET,
    ESS_REQ_APPROVED_GROUP 
            where HTH_DATE_OF_RESIGANTION BETWEEN to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy')
            and    ELR_SERVICE_TYPE = 30  
    and ELR_REF_DOC_NO = HTH_DOCUMENT_NO
    AND EMPID = ELR_EMP_ID AND EMPID = ELR_EMP_ID  
    AND UNIQUE_ID_1 = TO_CHAR(ELR_LEAVE_REQ_HEAD_ID)
    AND UNIQUE_ID_2 = TO_CHAR(ELR_LEAVE_REQ_HEAD_ID)
    and exists (
    SELECT 1 FROM AUTH_WF_HEADER, AUTH_WF_DOC_DETAIL
              WHERE
                    AWDD_WF_HEADER_ROW_ID = AWH_ROW_ID
   AND AWDD_EMP_APPROVED_BY = '" + empcode + @"' AND AWH_UNIQUE_ID1 = TO_CHAR(ELR_LEAVE_REQ_HEAD_ID) AND AWH_UNIQUE_ID2 = TO_CHAR(ELR_LEAVE_REQ_HEAD_ID))
    and((ELR_FROM_DT BETWEEN to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy') OR
        ELR_TO_DT BETWEEN to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy')) OR
            (ELR_FROM_DT < to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy') < ELR_TO_DT))
 ORDER BY ELR_LEAVE_REQ_HEAD_ID DESC  ";


            DataTable dt = new DataTable();
            dt = ObjDb.execute_query_retun_datatable(strquery);
            if (dt.Rows.Count > 0)
            {
                var JSONresult = JsonConvert.SerializeObject(dt);
                Response.Write(JSONresult);
            }
            else
            {
                Response.Write("{}");
            }

        }


        void ExitForm_History(string empcode)
        {
            var fromDate1 = Request["FromDate"].ToArabicDate();
            var toDate1 = Request["ToDate"].ToArabicDate();
            DBAccess ObjDb = new DBAccess();  

            string strquery = @" SELECT
      ELR_EMP_ID, ELR_SUBJ, ELR_BODY,TO_CHAR(ELR_REQUESTED_DT, 'dd/MM/yyyy')   
      ELR_REQUESTED_DT, ELR_REQ_STATUS_ID, ELR_SERVICE_TYPE,ELR_LEAVE_REQ_HEAD_ID,TO_CHAR(ELR_FROM_DT, 'dd/MM/yyyy') ELR_FROM_DT,
       EMPID, ENAME, BRANCH_NAME, DEPT_NAME, DIV_NAME,DESIGNATION_DESC,CASE WHEN ELR_REQ_STATUS_ID ='11' THEN 'CANCELLED' ELSE APPROVED_GROUP_NAME END APPROVED_GROUP_NAME,
    APP_CNT ,TO_CHAR (ELR_APPROVED_DT, 'DD-MM-YYY hh24:mi') APP_DATE,
            TO_CHAR (ELR_APPROVED_DT, 'hh24:mi')APP_TIME FROM
    ESS_LEAVE_REQ,  VU_EMP_DET  ,
    ESS_REQ_APPROVED_GROUP,
         ( select AWDD_WF_HEADER_ROW_ID WF_HEADER_ID, SUM(CASE WHEN wd2.AWDD_STATUS in ('A','R') THEN 1 ELSE 0 END) AS APP_CNT
        from AUTH_WF_DOC_DETAIL wd2  
                    GROUP BY AWDD_WF_HEADER_ROW_ID)

            WHERE ELR_FROM_DT BETWEEN to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy')
        AND UNIQUE_ID_1 =  TO_CHAR(ELR_LEAVE_REQ_HEAD_ID)
        AND UNIQUE_ID_2 = TO_CHAR(ELR_LEAVE_REQ_HEAD_ID) 
        and WF_HEADER_ID(+) = AWH_ROW_ID1";

            strquery += @" AND ELR_SERVICE_TYPE = 39 
            AND EMPID = ELR_EMP_ID AND EMPID = '" + empcode + @"'
 and ((ELR_FROM_DT BETWEEN to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') OR
        ELR_TO_DT BETWEEN to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy')) OR 
          ( ELR_FROM_DT < to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') < ELR_TO_DT ))
   

            order by ELR_LEAVE_REQ_HEAD_ID desc";
            DataTable dt = new DataTable();
            dt = ObjDb.execute_query_retun_datatable(strquery);
            if (dt.Rows.Count > 0)
            {
                var JSONresult = JsonConvert.SerializeObject(dt);
                Response.Write(JSONresult);
            }
            else
            {
                Response.Write("{}");
            }

        }

        void ExitForm_Approved(string empcode)
        {
            var fromDate1 = Request["FromDate"].ToArabicDate();
            var toDate1 = Request["ToDate"].ToArabicDate();

            DBAccess ObjDb = new DBAccess();
             

            string strquery = @" SELECT 
    ELR_EMP_ID, ELR_SUBJ, ELR_BODY, TO_CHAR(ELR_REQUESTED_DT, 'dd/MM/yyyy')
    ELR_REQUESTED_DT, ELR_REQ_STATUS_ID, ELR_SERVICE_TYPE,ELR_LEAVE_REQ_HEAD_ID,
      EMPID, ENAME, BRANCH_NAME, DEPT_NAME, DIV_NAME,DESIGNATION_DESC,
    CASE WHEN ELR_REQ_STATUS_ID = 11  THEN          
            GET_LEAVE_STATUS (ELR_REQ_STATUS_ID)    
        ELSE  
            APPROVED_GROUP_NAME END APPROVED_GROUP_NAME  ,
    TO_CHAR (ELR_APPROVED_DT, 'DD-MM-YYY HH24:MI') APP_DATE,
            TO_CHAR (ELR_APPROVED_DT, 'hh24:mi')APP_TIME
FROM 
    ESS_LEAVE_REQ, VU_EMP_DET,
    ESS_REQ_APPROVED_GROUP 
            where   ELR_SERVICE_TYPE = 39 
    AND EMPID = ELR_EMP_ID AND EMPID = ELR_EMP_ID  
    AND UNIQUE_ID_1 = TO_CHAR(ELR_LEAVE_REQ_HEAD_ID)
    AND UNIQUE_ID_2 = TO_CHAR(ELR_LEAVE_REQ_HEAD_ID)
    and exists (
    SELECT 1 FROM AUTH_WF_HEADER, AUTH_WF_DOC_DETAIL
              WHERE
                    AWDD_WF_HEADER_ROW_ID = AWH_ROW_ID
   AND AWDD_EMP_APPROVED_BY = '" + empcode + @"' AND AWH_UNIQUE_ID1 = TO_CHAR(ELR_LEAVE_REQ_HEAD_ID) AND AWH_UNIQUE_ID2 = TO_CHAR(ELR_LEAVE_REQ_HEAD_ID))
    and((ELR_FROM_DT BETWEEN to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy') OR
        ELR_TO_DT BETWEEN to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy')) OR
            (ELR_FROM_DT < to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy') < ELR_TO_DT))
 order by ELR_LEAVE_REQ_HEAD_ID desc  ";


            DataTable dt = new DataTable();
            dt = ObjDb.execute_query_retun_datatable(strquery);
            if (dt.Rows.Count > 0)
            {
                var JSONresult = JsonConvert.SerializeObject(dt);
                Response.Write(JSONresult);
            }
            else
            {
                Response.Write("{}");
            }

        }
        void DOWNLOAD_VIDEOS()
        {

            DBAccess ObjDb = new DBAccess();
            string strquery = @"SELECT ROW_NUMBER() OVER(ORDER BY AD_ROW_ID) SNO,
AD_ROW_ID,AD_MODULE_ID,AD_DOC_TITLE,TO_CHAR(AD_DATE,'DD/MM/YYYY')AD_DATE,'' AD_NO_OF_ATTH,
'' FILENAME,'' PATH FROM AMM_DOWNLOADS WHERE AD_IS_ACIVE='Y' AND NVL(AD_MODULE_ID ,'0') =  '-1' ORDER BY AD_ROW_ID";

            string strpath = Server.MapPath("~/Downloads");
            DirectoryInfo dir = new DirectoryInfo(strpath);
            DirectoryInfo[] subdir = dir.GetDirectories();

            DataTable dts = new DataTable();
            dts.Columns.Add("Path");
            dts.Columns.Add("Title");



            DataTable dt = new DataTable();
            dt = ObjDb.execute_query_retun_datatable(strquery);
            if (dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    if (dr["AD_ROW_ID"].ToString() != "")
                    {
                        int noOfatth = 0;
                        var str = "";

                        var title = dr["AD_DOC_TITLE"].ToString();

                        var filePath = "~/Downloads/" + dr["AD_ROW_ID"].ToString() + "/";
                        string fname = "";
                        if (Directory.Exists(Server.MapPath(filePath)))
                        {
                            string[] filenames = Directory.GetFiles(MapPath(filePath));
                            if (filenames != null && filenames.Length > 0)
                            {
                                for (int cnt = 0; cnt < filenames.Length; cnt++)
                                {
                                    var fi = new FileInfo(filenames[cnt]);

                                    if (str != "")
                                        str += ",";
                                    str = ResolveClientUrl("~/Downloads/" + dr["AD_ROW_ID"].ToString() + "/" + fi.Name.ToString());
                                    fname = fi.Name.ToString() + "|";
                                    noOfatth++;

                                    var ro = dts.NewRow();

                                    ro["Title"] = title;
                                    ro["Path"] = str;
                                    dts.Rows.Add(ro);
                                }

                            }

                        }
                        dr.SetField("AD_NO_OF_ATTH", noOfatth.ToString());
                        dr.SetField("PATH", str.ToString());
                        dr.SetField("FILENAME", fname.ToString());
                    }

                }
            }

            var JSONresult = JsonConvert.SerializeObject(dts);
            Response.Write(JSONresult);

        }

        void ESS_WF_DELETE_LINEITEMS()
        {
            DBAccess db = new DBAccess();
            var WFDetId = Request["WFDetId"];
            var EMPCode = Request["EMPCode"];
            var strdata = ""; int insertResult;
            if (WFDetId != "")
            {  // delete

                strdata = @"delete from  Auth_WF_Approver_Log where AWAL_WF_DET_ROW_ID="+ WFDetId+" and AWAL_EMP_CODE='" + EMPCode + "'";
                 insertResult = db.execute_query(strdata);

 

                if (insertResult>0)
                {
                    WriteSuccessMessage(1.ToString());
                }
            }
            else
                WriteSuccessMessage("");
        }

        void GENERATEWORKFLOW(string vRequestId)
        {

            var _params = new List<ERP1.Pro_Parameters>();
            _params.Add(new ERP1.Pro_Parameters { _par_name = "P_REQ_ID", _par_value = vRequestId });
            _params.Add(new ERP1.Pro_Parameters { _par_name = "P_TYPE", _par_value = "" });
            var statusText = new ERP1.Pro_Parameters { _par_name = "P_STATUS_TEXT", _par_value = "", _par_in_out = "OUT", _par_data_type = AppCode.Pro_Parameters.ParameterType.Varchar2.ToString() };

            _params.Add(statusText);
            db.executeProcedure("ESS_WORKFLOW_DELETE_DOCS", _params);

            if (statusText._par_value != null || !string.IsNullOrEmpty(statusText._par_value.ToString()))
            {

                string updateQ = @"UPDATE ESS_LEAVE_REQ SET MTH_SEND_FOR_APPROVAL='N' WHERE ELR_LEAVE_REQ_HEAD_ID=" + vRequestId + "";

                var cnt = objDB.execute_query(updateQ);
                if (cnt > 0)
                {
                    string updateS = @"UPDATE ESS_LEAVE_REQ SET MTH_SEND_FOR_APPROVAL='Y' WHERE ELR_LEAVE_REQ_HEAD_ID=" + vRequestId + "";
                    objDB.execute_query(updateS);

                }

                WriteSuccessMessage("1");
            }

        }
        void ESS_WF_DELETE()
        {
            DBAccess db = new DBAccess();
            var Reqid = Request["Req_id"];
            var ReqType = Request["Req_type"];


            if (Reqid != "")
            {  // delete

                //                strdata=@"delete from  Auth_WF_Approver_Log where  exists
                //(select 1 from Auth_WF_Doc_Detail where AWDD_ROW_ID = AWAL_WF_DET_ROW_ID and
                //exists(select 1 from Auth_WF_Header where AWH_UNIQUE_ID1 = '" + Reqid + @"' and AWH_ROW_ID = AWDD_WF_HEADER_ROW_ID))";
                //                log_error.write_to_log_file("Workflow Delete", "false", strdata);
                //                insertResult = db.execute_query(strdata);


                var _params = new List<ERP1.Pro_Parameters>();
                _params.Add(new ERP1.Pro_Parameters { _par_name = "P_REQ_ID", _par_value = Reqid });
                _params.Add(new ERP1.Pro_Parameters { _par_name = "P_TYPE", _par_value = ReqType });
                var statusText = new ERP1.Pro_Parameters { _par_name = "P_STATUS_TEXT", _par_value = "",_par_in_out = "OUT", _par_data_type = AppCode.Pro_Parameters.ParameterType.Varchar2.ToString() };

                _params.Add(statusText);
                db.executeProcedure("ESS_WORKFLOW_DELETE_DOCS", _params);

                if (statusText._par_value != null || !string.IsNullOrEmpty(statusText._par_value.ToString()))
                {
                    WriteSuccessMessage(statusText._par_value.ToString());
                }
            }
            else
                WriteSuccessMessage("");


        }
        void INV_WF_DELETE()
        {
            DBAccess db = new DBAccess();
            var DOCNO = Request["Req_id"];



            if (DOCNO != "")
            {

                var _params = new List<ERP1.Pro_Parameters>();
                _params.Add(new ERP1.Pro_Parameters { _par_name = "P_DOC_NO", _par_value = DOCNO });
                var statusText = new ERP1.Pro_Parameters { _par_name = "P_STATUS_TEXT", _par_value = "", _par_in_out = "OUT", _par_data_type = AppCode.Pro_Parameters.ParameterType.Varchar2.ToString() };

                _params.Add(statusText);
                db.executeProcedure("INV_WORKFLOW_DELETE_DOCS", _params);

                if (statusText._par_value != null || !string.IsNullOrEmpty(statusText._par_value.ToString()))
                {
                    WriteSuccessMessage(statusText._par_value.ToString());
                }
            }
            else
                WriteSuccessMessage("");


        }
        void Empdownloads(string empcode)
        {

            DBAccess ObjDb = new DBAccess();
            string strquery = @"SELECT ROW_NUMBER() OVER(ORDER BY AD_ROW_ID) SNO,
AD_ROW_ID,AD_MODULE_ID,AD_DOC_TITLE,TO_CHAR(AD_DATE,'DD/MM/YYYY')AD_DATE,'' AD_NO_OF_ATTH,
'' FILENAME,'' PATH FROM AMM_DOWNLOADS WHERE AD_IS_ACIVE='Y' AND NVL(AD_MODULE_ID ,'0') <>  '-1' ORDER BY AD_ROW_ID";
             


            string strpath = Server.MapPath("~/Downloads");
            DirectoryInfo dir = new DirectoryInfo(strpath);
            DirectoryInfo[] subdir = dir.GetDirectories();
            //if (!IsPostBack)
            //{
            DataTable dts = new DataTable();
            dts.Columns.Add("dirname");
            dts.Columns.Add("fname");
            dts.Columns.Add("path");
            dts.Columns.Add("no");



            DataTable dt = new DataTable();
            dt = ObjDb.execute_query_retun_datatable(strquery);
            if (dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    if (dr["AD_ROW_ID"].ToString() != "")
                    {
                        int noOfatth = 0;
                        var str = "";

                        var filePath = "~/Downloads/" + dr["AD_ROW_ID"].ToString() + "/";
                        //  strpath = Server.MapPath(filePath);
                        string fname = "";
                        if (Directory.Exists(Server.MapPath(filePath)))
                        {
                            string[] filenames = Directory.GetFiles(MapPath(filePath));
                            if (filenames != null && filenames.Length > 0)
                            {
                                for (int cnt = 0; cnt < filenames.Length; cnt++)
                                {
                                    var fi = new FileInfo(filenames[cnt]);
                                    if (str != "")
                                        str += ",";
                                    str += ResolveClientUrl("~/Downloads/" + dr["AD_ROW_ID"].ToString() + "/" + fi.Name.ToString());
                                    fname += fi.Name.ToString() + "|";
                                    noOfatth++;
                                }

                            }

                        }
                        dr.SetField("AD_NO_OF_ATTH", noOfatth.ToString());
                        dr.SetField("PATH", str.ToString());
                        dr.SetField("FILENAME", fname.ToString());
                    }

                }
            }

            var JSONresult = JsonConvert.SerializeObject(dt);
            Response.Write(JSONresult);
            

        }



        void LOAD_MY_OTDETAILS(string empcode)
        {
            var from = Request["From"].ToArabicDate();
            var to = Request["To"].ToArabicDate();
            DBAccess ObjDb = new DBAccess(); essLeaveRequestDA = new EssLeaveRequestDA();
            string strquery = @"SELECT     TAS_ID, EMPID, ENAME,     BRANCH_CODE, BRANCH_NAME, DEPT_CODE, 
    DEPT_NAME, DESIG_CODE, DESIGNATION_CODE,     DESIGNATION_DESC,PEMP_EMP_OT_ELIGIBLE_FLAG,EORH_YYYYMM,
 TO_CHAR(EORH_CUTOFF_START_DATE,'DD/MM/YYYY') EORH_CUTOFF_START_DATE,
TO_CHAR(EORH_CUTOFF_END_DATE,'DD/MM/YYYY') EORH_CUTOFF_END_DATE,APP_CNT,
CASE WHEN ELR_REQ_STATUS_ID ='11' THEN 'CANCELLED' ELSE APPROVED_GROUP_NAME END APPROVED_GROUP_NAME,ELR_LEAVE_REQ_HEAD_ID,to_char(ELR_REQUESTED_DT,'dd/mm/yyyy')REQ_DATE ,
TO_CHAR (ELR_APPROVED_DT, 'DD-MM-YYY hh24:mi') APP_DATE,
            TO_CHAR (ELR_APPROVED_DT, 'hh24:mi')APP_TIME,ELR_REQ_STATUS_ID
  FROM 
    VU_EMP_DET,PPM_EMPLOYEE_DETAILS , ESS_OT_REQ_HEAD, 
     ESS_LEAVE_REQ L,ESS_REQ_APPROVED_GROUP , 
 ( select AWDD_WF_HEADER_ROW_ID WF_HEADER_ID, SUM(CASE WHEN wd2.AWDD_STATUS in ('A','R') THEN 1 ELSE 0 END) AS APP_CNT
        from AUTH_WF_DOC_DETAIL wd2  
                    GROUP BY AWDD_WF_HEADER_ROW_ID)

   WHERE PEMP_EMP_CODE = EMPID  AND PEMP_EMP_CODE = EORH_EMP_ID   
 AND EORH_EMP_ID=" + empcode + @" AND COMP_CODE ='" + user.Login_Company_Code + @"' 
   and PEMP_EMP_CODE=ELR_EMP_ID
    and ELR_SERVICE_TYPE=16  
     AND TO_CHAR(EORH_OT_REQ_HEAER_ID)=ELR_REF_DOC_NO 
      AND UNIQUE_ID_1 = TO_CHAR(ELR_LEAVE_REQ_HEAD_ID)
    AND UNIQUE_ID_2 = TO_CHAR(ELR_LEAVE_REQ_HEAD_ID) 
    AND WF_HEADER_ID(+) = AWH_ROW_ID1

      and((EORH_CUTOFF_START_DATE BETWEEN to_date('" + from.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy') AND to_date('" + to.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy') OR
        EORH_CUTOFF_END_DATE BETWEEN to_date('" + from.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy') AND to_date('" + to.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy')) OR
            (EORH_CUTOFF_START_DATE < to_date('" + from.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy') AND to_date('" + to.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy') < EORH_CUTOFF_END_DATE))
    order by ELR_LEAVE_REQ_HEAD_ID desc
     ";

            DataTable dt = new DataTable();
            dt = ObjDb.execute_query_retun_datatable(strquery);
            if (dt.Rows.Count > 0)
            {
                var JSONresult = JsonConvert.SerializeObject(dt);
                Response.Write(JSONresult);
            }
            else
            {
                ResponseWrite("");
            }

        }


        void LOAD_MY_OTAPPROVED(string empcode)
        {
            var from = Request["From"].ToArabicDate();
            var to = Request["To"].ToArabicDate();
            DBAccess ObjDb = new DBAccess(); essLeaveRequestDA = new EssLeaveRequestDA();
            string strquery = @"SELECT     TAS_ID, EMPID, ENAME,     BRANCH_CODE, BRANCH_NAME, DEPT_CODE, 
    DEPT_NAME, DESIG_CODE, DESIGNATION_CODE,     DESIGNATION_DESC,PEMP_EMP_OT_ELIGIBLE_FLAG,EORH_YYYYMM,
 TO_CHAR(EORH_CUTOFF_START_DATE,'DD/MM/YYYY')EORH_CUTOFF_START_DATE,
TO_CHAR(EORH_CUTOFF_END_DATE,'DD/MM/YYYY')EORH_CUTOFF_END_DATE,
APPROVED_GROUP_NAME,ELR_LEAVE_REQ_HEAD_ID ,to_char(ELR_REQUESTED_DT,'dd/mm/yyyy')REQ_DATE,
TO_CHAR (ELR_APPROVED_DT, 'DD-MM-YYY hh24:mi') APP_DATE,
            TO_CHAR (ELR_APPROVED_DT, 'hh24:mi')APP_TIME
  FROM 
    VU_EMP_DET,PPM_EMPLOYEE_DETAILS , ESS_OT_REQ_HEAD, 
     ESS_LEAVE_REQ L,ESS_REQ_APPROVED_GROUP  


   WHERE PEMP_EMP_CODE = EMPID  AND PEMP_EMP_CODE = EORH_EMP_ID   
  AND COMP_CODE ='" + user.Login_Company_Code + @"' 
   and PEMP_EMP_CODE=ELR_EMP_ID
    and ELR_SERVICE_TYPE=16  
     AND TO_CHAR(EORH_OT_REQ_HEAER_ID)=ELR_REF_DOC_NO 
      AND UNIQUE_ID_1 = TO_CHAR(ELR_LEAVE_REQ_HEAD_ID)
    AND UNIQUE_ID_2 = TO_CHAR(ELR_LEAVE_REQ_HEAD_ID)  
 
     and((EORH_CUTOFF_START_DATE BETWEEN to_date('" + from.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy') AND to_date('" + to.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy') OR
        EORH_CUTOFF_END_DATE BETWEEN to_date('" + from.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy') AND to_date('" + to.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy')) OR
            (EORH_CUTOFF_START_DATE < to_date('" + from.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy') AND to_date('" + to.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy') < EORH_CUTOFF_END_DATE))

    and exists ( 
    SELECT 1 FROM AUTH_WF_HEADER, AUTH_WF_DOC_DETAIL
              WHERE     
                    AWDD_WF_HEADER_ROW_ID = AWH_ROW_ID
                    AND AWDD_EMP_APPROVED_BY = " + empcode + @"
                    AND AWH_UNIQUE_ID1 =  TO_CHAR(ELR_LEAVE_REQ_HEAD_ID)
                    AND AWH_UNIQUE_ID2 = TO_CHAR(ELR_LEAVE_REQ_HEAD_ID)
    )
       order by ELR_LEAVE_REQ_HEAD_ID desc
     ";

            DataTable dt = new DataTable();
            dt = ObjDb.execute_query_retun_datatable(strquery);
            if (dt.Rows.Count > 0)
            {
                var JSONresult = JsonConvert.SerializeObject(dt);
                Response.Write(JSONresult);
            }
            else
            {
                ResponseWrite("");
            }

        }

        void LOAD_MY_TIMESHEETDETAILS(string empcode)
        {
            //TO_CHAR(TTS_DATE,'DD/MM/YYYY')  START_DATE,TO_CHAR(TTS_DATE,'DD/MM/YYYY')  END_DATE,
            //AND TO_CHAR(EORH_OT_REQ_HEAER_ID)=ELR_REF_DOC_NO 
            var from = Request["From"].ToArabicDate();
            var to = Request["To"].ToArabicDate();
            DBAccess ObjDb = new DBAccess(); essLeaveRequestDA = new EssLeaveRequestDA();
            string strquery = @"SELECT  DISTINCT   TAS_ID, EMPID, ENAME,     BRANCH_CODE, BRANCH_NAME, DEPT_CODE, 
    DEPT_NAME, DESIG_CODE, DESIGNATION_CODE,     DESIGNATION_DESC,PEMP_EMP_OT_ELIGIBLE_FLAG,APP_CNT, 
APPROVED_GROUP_NAME,ELR_LEAVE_REQ_HEAD_ID,TO_CHAR (ELR_FROM_DT, 'DD-MM-YYYY')From_Date,TO_CHAR (ELR_TO_DT, 'DD-MM-YYYY')To_Date,to_char(ELR_REQUESTED_DT,'dd/mm/yyyy')REQ_DATE ,
TO_CHAR (ELR_APPROVED_DT, 'DD-MM-YYY HH24:MI') APP_DATE,ELR_REQ_STATUS_ID,
            TO_CHAR (ELR_APPROVED_DT, 'hh24:mi')APP_TIME
  FROM 
    VU_EMP_DET,PPM_EMPLOYEE_DETAILS , TAS_TIME_SHEET, 
     ESS_LEAVE_REQ L,ESS_REQ_APPROVED_GROUP , 
 ( select AWDD_WF_HEADER_ROW_ID WF_HEADER_ID, SUM(CASE WHEN wd2.AWDD_STATUS in ('A','R') THEN 1 ELSE 0 END) AS APP_CNT
        from AUTH_WF_DOC_DETAIL wd2  
                    GROUP BY AWDD_WF_HEADER_ROW_ID)

   WHERE PEMP_EMP_CODE = EMPID  AND PEMP_EMP_CODE = TTS_EMP_CODE   
 AND TTS_EMP_CODE=" + empcode + @" AND COMP_CODE ='" + user.Login_Company_Code + @"' 
   and PEMP_EMP_CODE=ELR_EMP_ID
    and ELR_SERVICE_TYPE=31      
      AND UNIQUE_ID_1 = TO_CHAR(ELR_LEAVE_REQ_HEAD_ID)
    AND UNIQUE_ID_2 = TO_CHAR(ELR_LEAVE_REQ_HEAD_ID) 
    AND WF_HEADER_ID(+) = AWH_ROW_ID1

      and((TTS_DATE BETWEEN to_date('" + from.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy') AND to_date('" + to.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy') OR
        TTS_DATE BETWEEN to_date('" + from.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy') AND to_date('" + to.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy')) OR
            (TTS_DATE < to_date('" + from.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy') AND to_date('" + to.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy') < TTS_DATE))
    order by ELR_LEAVE_REQ_HEAD_ID desc
     ";

            DataTable dt = new DataTable();
            dt = ObjDb.execute_query_retun_datatable(strquery);
            if (dt.Rows.Count > 0)
            {
                var JSONresult = JsonConvert.SerializeObject(dt);
                Response.Write(JSONresult);
            }
            else
            {
                Response.Write("{}");
            }

        }


        void LOAD_MY_TIMESHEETAPPROVED(string empcode)
        {
            var from = Request["From"].ToArabicDate();
            var to = Request["To"].ToArabicDate();
            DBAccess ObjDb = new DBAccess(); essLeaveRequestDA = new EssLeaveRequestDA();
            string strquery = @"SELECT  DISTINCT   TAS_ID, EMPID, ENAME,     BRANCH_CODE, BRANCH_NAME, DEPT_CODE, 
    DEPT_NAME, DESIG_CODE, DESIGNATION_CODE,     DESIGNATION_DESC,PEMP_EMP_OT_ELIGIBLE_FLAG, 
APPROVED_GROUP_NAME,ELR_LEAVE_REQ_HEAD_ID,TO_CHAR (ELR_FROM_DT, 'DD-MM-YYYY')From_Date,TO_CHAR (ELR_TO_DT, 'DD-MM-YYYY')To_Date ,to_char(ELR_REQUESTED_DT,'dd/mm/yyyy')REQ_DATE,
TO_CHAR (ELR_APPROVED_DT, 'DD-MM-YYY HH24:MI') APP_DATE,
            TO_CHAR (ELR_APPROVED_DT, 'hh24:mi')APP_TIME
  FROM 
    VU_EMP_DET,PPM_EMPLOYEE_DETAILS , TAS_TIME_SHEET, 
     ESS_LEAVE_REQ L,ESS_REQ_APPROVED_GROUP 

   WHERE PEMP_EMP_CODE = EMPID  AND PEMP_EMP_CODE = TTS_EMP_CODE 
  AND COMP_CODE ='" + user.Login_Company_Code + @"' 
   and PEMP_EMP_CODE=ELR_EMP_ID
    and ELR_SERVICE_TYPE=31    
      AND UNIQUE_ID_1 = TO_CHAR(ELR_LEAVE_REQ_HEAD_ID)
    AND UNIQUE_ID_2 = TO_CHAR(ELR_LEAVE_REQ_HEAD_ID)  
 
     and((TTS_DATE BETWEEN to_date('" + from.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy') AND to_date('" + to.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy') OR
        TTS_DATE BETWEEN to_date('" + from.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy') AND to_date('" + to.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy')) OR
            (TTS_DATE < to_date('" + from.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy') AND to_date('" + to.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy') < TTS_DATE))

    and exists ( 
    SELECT 1 FROM AUTH_WF_HEADER, AUTH_WF_DOC_DETAIL
              WHERE     
                    AWDD_WF_HEADER_ROW_ID = AWH_ROW_ID
                    AND AWDD_EMP_APPROVED_BY = " + empcode + @"
                    AND AWH_UNIQUE_ID1 =  TO_CHAR(ELR_LEAVE_REQ_HEAD_ID)
                    AND AWH_UNIQUE_ID2 = TO_CHAR(ELR_LEAVE_REQ_HEAD_ID)
    )
       order by ELR_LEAVE_REQ_HEAD_ID desc
     ";

            DataTable dt = new DataTable();
            dt = ObjDb.execute_query_retun_datatable(strquery);
            if (dt.Rows.Count > 0)
            {
                var JSONresult = JsonConvert.SerializeObject(dt);
                Response.Write(JSONresult);
            }
            else
            {
                Response.Write("{}");
            }

        }
        void LOAD_MATERIAL()
        {
            DBAccess ObjDb = new DBAccess(); essLeaveRequestDA = new EssLeaveRequestDA();
            string str_select = @"SELECT ROW_NUMBER() OVER(ORDER BY MMGM_MFG_NAME) SLRNO ,
            MMGM_PRODUCT_MDM_ID PRODUCTID,MMGM_MFG_NAME PRODUCTNAME,MMGM_TRANSACTION_TYPE TRANSACTIONTYPE,
            MMGM_DESCRIPTION DESCRIPTION,MMGM_OPCO_PRODUCT_CODE OPCOCODE  
             FROM MDM_MATERIAL_VIEW  ORDER BY MMGM_MFG_NAME";

            DataTable dt = new DataTable();
            dt = ObjDb.execute_query_retun_datatable(str_select);
            if (dt.Rows.Count > 0)
            {
                var JSONresult = JsonConvert.SerializeObject(dt);
                Response.Write(JSONresult);
            }
            else
            {
                Response.Write("{}");
            }
        }

        void LOAD_VENDOR()
        {
            DBAccess ObjDb = new DBAccess(); essLeaveRequestDA = new EssLeaveRequestDA();
            string str_select = @"SELECT ROW_NUMBER() OVER(ORDER BY MMGV_PARTY_NAME)SLRNO, MMGV_ROW_ID,
          MMGV_TRANSACTION_TYPE TRANSACTIONTYPE,
          MMGV_PARTY_NAME PARTYNAME,
          MMGV_ADDRESS_LINE1 ADDRESS,
          MMGV_OPCO_PARTY_CODE VENDORCODE,
          MMGV_CREATION_DATE ,
          MMGV_UPDATE_DATE           
             FROM MDM_VENDOR_VIEW ORDER BY  MMGV_PARTY_NAME";
            DataTable dt = new DataTable();
            dt = ObjDb.execute_query_retun_datatable(str_select);
            if (dt.Rows.Count > 0)
            {
                var JSONresult = JsonConvert.SerializeObject(dt);
                Response.Write(JSONresult);
            }
            else
            {
                Response.Write("{}");
            }
        }

        void LOAD_CUSTOMER()
        {
            DBAccess ObjDb = new DBAccess(); essLeaveRequestDA = new EssLeaveRequestDA();
            string str_select = @"SELECT ROW_NUMBER() OVER(ORDER BY MMGC_PARTY_NAME)SLRNO,MMGC_ROW_ID,
          MMGC_TRANSACTION_TYPE TRANSACTIONTYPE,
          MMGC_PARTY_NAME PARTYNAME,MMGC_OPCO_PARTY_CODE PARTYCODE,
          MMGC_CREATION_DATE,
          MMGC_UPDATE_DATE          
          FROM MDM_CUSTOMER ORDER BY MMGC_PARTY_NAME";

            DataTable dt = new DataTable();
            dt = ObjDb.execute_query_retun_datatable(str_select);
            if (dt.Rows.Count > 0)
            {
                var JSONresult = JsonConvert.SerializeObject(dt);
                Response.Write(JSONresult);
            }
            else
            {
                Response.Write("{}");
            }
        }

        void EDIT_VENDOR()
        {
            DBAccess ObjDb = new DBAccess(); essLeaveRequestDA = new EssLeaveRequestDA();
            string str_select = @"SELECT ROW_NUMBER() OVER(ORDER BY MMGV_PARTY_NAME)SLRNO, MMGV_ROW_ID VENDORID,
          MMGV_TRANSACTION_TYPE TRANSACTIONTYPE,
          MMGV_PARTY_NAME PARTYNAME,
          MMGV_ADDRESS_LINE1 ADDRESS,
          MMGV_OPCO_PARTY_CODE VENDORCODE,
          MMGV_CREATION_DATE ,
          MMGV_UPDATE_DATE           
             FROM MDM_VENDOR_VIEW ORDER BY  MMGV_PARTY_NAME";

            DataTable dt = new DataTable();
            dt = ObjDb.execute_query_retun_datatable(str_select);
            if (dt.Rows.Count > 0)
            {
                var JSONresult = JsonConvert.SerializeObject(dt);
                Response.Write(JSONresult);
            }
            else
            {
                Response.Write("");
            }
        }

        void EDIT_CUSTOMER()
        {
            DBAccess ObjDb = new DBAccess(); essLeaveRequestDA = new EssLeaveRequestDA();
            string str_select = @"SELECT ROW_NUMBER() OVER(ORDER BY MMGC_PARTY_NAME)SLRNO,MMGC_ROW_ID,
          MMGC_TRANSACTION_TYPE TRANSACTIONTYPE,
          MMGC_PARTY_NAME PARTYNAME,MMGC_OPCO_PARTY_CODE PARTYCODE,
          MMGC_CREATION_DATE,
          MMGC_UPDATE_DATE          
          FROM MDM_CUSTOMER ORDER BY MMGC_PARTY_NAME";

            DataTable dt = new DataTable();
            dt = ObjDb.execute_query_retun_datatable(str_select);
            if (dt.Rows.Count > 0)
            {
                var JSONresult = JsonConvert.SerializeObject(dt);
                Response.Write(JSONresult);
            }
            else
            {
                Response.Write("{}");
            }
        }


        void LOAD_MY_LOAN()
        {
            DBAccess ObjDb = new DBAccess();
            string empCode = Request["EmpCode"];
            var fromDate1 = Request["FromDate"].ToArabicDate();
            var toDate1 = Request["ToDate"].ToArabicDate();
            DataTable dt1 = objDB.execute_query_retun_datatable(@"

SELECT 
    ELR_LEAVE_REQ_HEAD_ID,
    ELR_EMP_ID, ELR_LEAVE_ID, to_char(ELR_FROM_DT,'dd/mm/yyyy') ELR_FROM_DT1, 
    to_char(ELR_TO_DT,'dd/mm/yyyy') ELR_TO_DT1, ELR_TOT_DAYS, ELR_BODY, 
    ELR_REQ_STATUS_ID, 
    EMPID, ENAME,PLTM_LEAVE_TYPE_DESC,
    BRANCH_NAME, DEPT_NAME, DIV_NAME, DESIGNATION_DESC,APPROVED_GROUP_NAME,
    APP_CNT,to_char(ELR_REQUESTED_DT,'dd/mm/yyyy')REQ_DATE ,
    ELR_LOAN_LOAN_TYPE, ELR_LOAN_REFERENCE, 
    trim( to_char(ELR_LOAN_AMOUNT,'999999999999999D999')) 
    ELR_LOAN_AMOUNT, trim( to_char(ELR_LOAN_MONTHLY_INSTALMENT,'999999999999999D999'))   ELR_LOAN_MONTHLY_INSTALMENT, 
    ELR_LOAN_NO_OF_TENURE,
    to_char( ELR_LOAN_START_DATE,'dd/mm/yyyy') ELR_LOAN_START_DATE, to_char( ELR_LOAN_CLOSING_DATE ,'dd/mm/yyyy') ELR_LOAN_CLOSING_DATE,
    PLTM_LOAN_TYPE_DESC,TO_CHAR (ELR_APPROVED_DT, 'DD-MM-YYY hh24:mi') APP_DATE,
            TO_CHAR (ELR_APPROVED_DT, 'hh24:mi')APP_TIME
FROM 
    ESS_LEAVE_REQ,ppm_leave_type_master,
    vu_emp_det,ESS_REQ_APPROVED_GROUP,
      ( select 
            AWDD_WF_HEADER_ROW_ID WF_HEADER_ID, SUM(CASE WHEN wd2.AWDD_STATUS in ('A','R') THEN 1 ELSE 0 END) AS APP_CNT
  from 
        AUTH_WF_DOC_DETAIL wd2  
        GROUP BY 
    AWDD_WF_HEADER_ROW_ID), PPM_LOAN_TYPE_MASTER 

    
where
    empid=elr_emp_id
    and ELR_SERVICE_TYPE=10
    AND ELR_LEAVE_ID = PLTM_LEAVE_TYPE_CODE(+)
     AND UNIQUE_ID_1 =  TO_CHAR(ELR_LEAVE_REQ_HEAD_ID)
                    AND UNIQUE_ID_2 = TO_CHAR(ELR_LEAVE_REQ_HEAD_ID) 
    and WF_HEADER_ID(+) = AWH_ROW_ID1
    
    AND ELR_EMP_ID = '" + empCode + @"'
    and PLTM_LOAN_TYPE_CODE = ELR_LOAN_LOAN_TYPE
 and ((ELR_FROM_DT BETWEEN to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') OR
        ELR_TO_DT BETWEEN to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy')) OR 
          ( ELR_FROM_DT < to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') < ELR_TO_DT ))
   order by ELR_LEAVE_REQ_HEAD_ID desc
");
            if (dt1 != null && dt1.Rows.Count > 0)
            {
                var JSONresult = JsonConvert.SerializeObject(dt1);
                Response.Write(JSONresult);
            }
            else
            {
                Response.Write("{}");
            }
        }

        void LOAD_MY_LOANAPPROVED()
        {
            DBAccess ObjDb = new DBAccess();
            string empCode = Request["EmpCode"];
            var fromDate1 = Request["FromDate"].ToArabicDate();
            var toDate1 = Request["ToDate"].ToArabicDate();
            DataTable dt1 = objDB.execute_query_retun_datatable(@"

SELECT 
    ELR_LEAVE_REQ_HEAD_ID,
    ELR_EMP_ID, ELR_LEAVE_ID, to_char(ELR_FROM_DT,'dd/mm/yyyy') ELR_FROM_DT1, 
    to_char(ELR_TO_DT,'dd/mm/yyyy') ELR_TO_DT1, ELR_TOT_DAYS, ELR_BODY, 
    ELR_REQ_STATUS_ID, 
    EMPID, ENAME,PLTM_LEAVE_TYPE_DESC,
    BRANCH_NAME, DEPT_NAME, DIV_NAME, DESIGNATION_DESC,APPROVED_GROUP_NAME,
    APP_CNT,to_char(ELR_REQUESTED_DT,'dd/mm/yyyy')REQ_DATE ,
    ELR_LOAN_LOAN_TYPE, ELR_LOAN_REFERENCE, 
    trim( to_char(ELR_LOAN_AMOUNT,'999999999999999D999')) 
    ELR_LOAN_AMOUNT, trim( to_char(ELR_LOAN_MONTHLY_INSTALMENT,'999999999999999D999'))   ELR_LOAN_MONTHLY_INSTALMENT, 
    ELR_LOAN_NO_OF_TENURE,
    to_char( ELR_LOAN_START_DATE,'dd/mm/yyyy') ELR_LOAN_START_DATE, to_char( ELR_LOAN_CLOSING_DATE ,'dd/mm/yyyy') ELR_LOAN_CLOSING_DATE,
    PLTM_LOAN_TYPE_DESC,TO_CHAR (ELR_APPROVED_DT, 'DD-MM-YYY hh24:mi') APP_DATE,
            TO_CHAR (ELR_APPROVED_DT, 'hh24:mi')APP_TIME
FROM 
    ESS_LEAVE_REQ,ppm_leave_type_master,
    vu_emp_det,ESS_REQ_APPROVED_GROUP,
      ( select 
            AWDD_WF_HEADER_ROW_ID WF_HEADER_ID, SUM(CASE WHEN wd2.AWDD_STATUS in ('A','R') THEN 1 ELSE 0 END) AS APP_CNT
  from 
        AUTH_WF_DOC_DETAIL wd2  
        GROUP BY 
    AWDD_WF_HEADER_ROW_ID), PPM_LOAN_TYPE_MASTER 

    
where
    empid=elr_emp_id
    and ELR_SERVICE_TYPE=10
    AND ELR_LEAVE_ID = PLTM_LEAVE_TYPE_CODE(+)
     AND UNIQUE_ID_1 =  TO_CHAR(ELR_LEAVE_REQ_HEAD_ID)
                    AND UNIQUE_ID_2 = TO_CHAR(ELR_LEAVE_REQ_HEAD_ID) 
    and WF_HEADER_ID(+) = AWH_ROW_ID1   
    and PLTM_LOAN_TYPE_CODE = ELR_LOAN_LOAN_TYPE
    and exists ( 
    SELECT 1 FROM AUTH_WF_HEADER, AUTH_WF_DOC_DETAIL
              WHERE     
                    AWDD_WF_HEADER_ROW_ID = AWH_ROW_ID
                    AND AWDD_EMP_APPROVED_BY = '" + empCode + @"'
                    AND AWH_UNIQUE_ID1 =  TO_CHAR(ELR_LEAVE_REQ_HEAD_ID)
                    AND AWH_UNIQUE_ID2 = TO_CHAR(ELR_LEAVE_REQ_HEAD_ID)
    )

and ELR_REQUESTED_DT BETWEEN to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy')

 order by ELR_LEAVE_REQ_HEAD_ID desc  
");
            if (dt1 != null && dt1.Rows.Count > 0)
            {
                var JSONresult = JsonConvert.SerializeObject(dt1);
                Response.Write(JSONresult);
            }
            else
            {
                Response.Write("{}");
            }
        }

        bool LoanEligibility(out string statusMsg, string essloantype, DateTime startdate)
        {
            statusMsg = "";

            DBAccess db = new DBAccess();
            string empcode = Request["EmpCode"];
            string loantype = "";
            var startDate = DateTime.Now;
            if (string.IsNullOrEmpty(Request["ELR_LOAN_START_DATE"]))
            {
                startDate = startdate;// Request["ELR_LOAN_START_DATE"].ToArabicDate();
            }
            else
            {
                startDate = Request["ELR_LOAN_START_DATE"].ToArabicDate();
            }
            if (string.IsNullOrEmpty(Request["LoanType"]))
            {
                loantype = essloantype;
            }
            else
            {
                loantype = Request["LoanType"];
            }

            var _params = new List<ERP1.Pro_Parameters>();
            _params.Add(new ERP1.Pro_Parameters { _par_name = "P_COMP_CODE", _par_value = "01" });
            _params.Add(new ERP1.Pro_Parameters { _par_name = "P_EMP_ID", _par_value = Request["EmpCode"] });
            _params.Add(new ERP1.Pro_Parameters { _par_name = "P_LOAN_TYPE", _par_value = loantype });
            _params.Add(new ERP1.Pro_Parameters { _par_name = "P_LOAN_DATE", _par_value = startDate });

            var eligible = new ERP1.Pro_Parameters { _par_name = "P_IS_ELIGIBLE", _par_value = "", _par_in_out = "OUT", _par_data_type = AppCode.Pro_Parameters.ParameterType.Varchar2.ToString() };
            var maxLoanAmt = new ERP1.Pro_Parameters { _par_name = "P_MAX_LOAN", _par_value = 0D, _par_in_out = "OUT", _par_data_type = AppCode.Pro_Parameters.ParameterType.Decimal.ToString() };
            var maxTenure = new ERP1.Pro_Parameters { _par_name = "P_MAX_TENURE", _par_value = 0D, _par_in_out = "OUT", _par_data_type = AppCode.Pro_Parameters.ParameterType.Decimal.ToString() };
            var disClaimer = new ERP1.Pro_Parameters { _par_name = "P_DISCLAIMER", _par_value = "", _par_in_out = "OUT", _par_data_type = AppCode.Pro_Parameters.ParameterType.Varchar2.ToString() };

            var statusText = new ERP1.Pro_Parameters { _par_name = "P_STATUS_TEXT", _par_value = "", _par_in_out = "OUT", _par_data_type = AppCode.Pro_Parameters.ParameterType.Varchar2.ToString() };

            _params.Add(eligible);
            _params.Add(maxLoanAmt);
            _params.Add(maxTenure);
            _params.Add(disClaimer);
            _params.Add(statusText);

            //AppCode.LogError.WriteToLogFile("PAY_GET_LOAN_ELIG_DETAILS:" +
            //    @"P_EMP_ID, _par_value = Request[EmpCode]" + Request["EmpCode"] +
            //    @"P_LOAN_TYPE, _par_value = Request[P_LOAN_TYPE]" + Request["LoanType"] +
            //    @"P_LOAN_DATE, _par_value = Request[P_LOAN_DATE]" + startDate +
            //    @"P_IS_ELIGIBLE, _par_value = Request[P_LOAN_DATE]" +
            //    @"P_STATUS_TEXT, _par_value = Request[P_LOAN_DATE]"
            //    , false, ""
            //    );
            db.executeProcedure("PAY_GET_LOAN_ELIG_DETAILS", _params);

            if (!string.IsNullOrEmpty(_params[0]._err_string))
            {
                Response.Write(APIResult<string>.ErrorResponse("Error: " + _params[0]._err_string).GetResponse());
                return false;
            }

            errorText = statusText._par_value.ToString();
            var outputResult = @"{ ""IS_ELIGIBLE"": """ + eligible._par_value + @""", ""P_STATUS_TEXT"": """ + statusText._par_value + @""", ""MAX_LOAN"": """ + maxLoanAmt._par_value + @""", ""MAX_TENURE"": """ +
                maxTenure._par_value + @""",
""DISCLAIMER"": """ + disClaimer._par_value.ToString().Replace("'", "") + @"""}";

            statusMsg = outputResult;
            if (eligible._par_value != null && eligible._par_value.ToString() == "YES")
            {
                return true;
            }
            return false;
            // Response.Write(outputResult);
        }
        string errorText = "";


        void SaveLoan()
        {


            essLeaveRequestDA = new EssLeaveRequestDA();
            essLeaveRequestHeader.LeaveReqDetails.Add(new EssLeaveRequest
            {
                ELR_LEAVE_REQ_ID = (essLeaveRequestHeader.LeaveReqDetails.Count()
                > 0 ? essLeaveRequestHeader.LeaveReqDetails.Max(m => m.ELR_LEAVE_REQ_ID) + 1 : 1),
                ELR_SERVICE_TYPE = ESSServices.LoanRequest
            });

            EssLeaveRequest ess = essLeaveRequestHeader.LeaveReqDetails[0];


            ess.ELR_LOAN_AMOUNT = Request.Form["ELR_LOAN_AMOUNT"].ToDouble();
            ess.ELR_LOAN_LOAN_TYPE = Request.Form["ELR_LOAN_LOAN_TYPE"];
            ess.ELR_LOAN_NO_OF_TENURE = Request.Form["ELR_LOAN_NO_OF_TENURE"].ToInt();
            ess.ELR_FROM_DT = ess.ELR_LOAN_START_DATE = Request.Form["ELR_LOAN_START_DATE"].ToArabicDate();
            ess.ELR_TO_DT = ess.ELR_LOAN_CLOSING_DATE = Request.Form["ELR_LOAN_CLOSING_DATE"].ToArabicDate();
            ess.ELR_LOAN_REFERENCE = Request.Form["ELR_LOAN_REFERENCE"];
            ess.ELR_LOAN_MONTHLY_INSTALMENT = Request.Form["ELR_LOAN_MONTHLY_INSTALMENT"].ToDouble();



            //if (string.IsNullOrEmpty(ess.ELR_LOAN_REFERENCE))
            //{
            //    Response.Write("Error: Choose the Loan Reference.");
            //    return;
            //}
            if (string.IsNullOrEmpty(ess.ELR_LOAN_LOAN_TYPE))
            {
                Response.Write(APIResult<string>.ErrorResponse("Error: Choose the Loan Type.").GetResponse());
                return;
            }
            if (ess.ELR_LOAN_AMOUNT <= 0)
            {
                Response.Write(APIResult<string>.ErrorResponse("Error: Enter the loan amount.").GetResponse());
                return;
            }
            //if (string.IsNullOrEmpty(ess.ELR_LOAN_CLOSING_DATE) || string.IsNullOrEmpty(ELR_LOAN_START_DATE.Text))
            //{
            //    Response.Write("Enter the loan date.");
            //    return false;
            //}
            if (ess.ELR_LOAN_NO_OF_TENURE <= 0)
            {
                Response.Write(APIResult<string>.ErrorResponse("Error: Enter the loan tenure.").GetResponse());
                return;
            }
            if (ess.ELR_LOAN_MONTHLY_INSTALMENT <= 0)
            {
                Response.Write(APIResult<string>.ErrorResponse("Error: Enter the loan installment.").GetResponse());
                return;
            }

            dbaccess objDB = new dbaccess();


            //check whether loan is already applied
            if (ESSHelperFunctions.IsLoanAlreadyApplied(ess.ELR_EMP_ID,
                ess.ELR_LOAN_LOAN_TYPE, ess.ELR_LOAN_START_DATE.ToShortDateString(), ess.ELR_LOAN_AMOUNT))
            {
                Response.Write(APIResult<string>.ErrorResponse("Selected Loan type is already applied for the period mentioned.").GetResponse());
                return;
            }

            ess.ELR_LEAVE_ID = "";
            ess.ELR_FROM_DT = Request.Form["ELR_LOAN_START_DATE"].ToArabicDate();
            ess.ELR_TO_DT = Request.Form["ELR_LOAN_CLOSING_DATE"].ToArabicDate();
            ess.AttendacneOtherReasonDesc = ess.ELR_SUBJ = ess.ELR_BODY = "Loan Request";
            ess.AttendacneReasonCode = "";
            //ess.ELR_NOTIFY_TO = Request.Form["empActingHidden"];



            ess.ELR_EMP_ID = Request.Form["EMPCode"];

            essLeaveRequestHeader.ELR_FROM_DT = ess.ELR_FROM_DT;
            essLeaveRequestHeader.ELR_TO_DT = ess.ELR_TO_DT;
            essLeaveRequestHeader.ELR_LEAVE_ID = ess.ELR_LEAVE_ID;
            essLeaveRequestHeader.ELR_SUBJ = ess.ELR_SUBJ;
            essLeaveRequestHeader.ELR_BODY = ess.ELR_BODY = Request["HTH_COMMENTS"];

            essLeaveRequestHeader.ELR_EMP_ID = ess.ELR_EMP_ID;


            essLeaveRequestHeader.ELR_SERVICE_TYPE = ESSServices.LoanRequest;
            ess.ELR_SERVICE_TYPE = ESSServices.LoanRequest;

            essLeaveRequestHeader.ELR_CREATED_USER_ID = Request.Form["UserId"];

            if (ess.ELR_FROM_DT == null || ess.ELR_TO_DT == null)
            {
                Response.Write(APIResult<string>.ErrorResponse("Error: Enter the from date and to date values").GetResponse());
                return;
            }
            if (ess.ELR_FROM_DT.Value > ess.ELR_TO_DT.Value)
            {
                Response.Write(APIResult<string>.ErrorResponse("Error: From date should be greater than to date.").GetResponse());
                return;
            }

            ess.ELR_TOT_DAYS = essLeaveRequestHeader.ELR_TOT_DAYS = (ess.ELR_TO_DT.Value - ess.ELR_FROM_DT.Value).TotalDays;

            if (objDB.execute_scalar(@"
SELECT count(*) FROM ESS_LEAVE_REQ
    WHERE  ELR_SERVICE_TYPE = 10
    AND ELR_EMP_ID = '" + ess.ELR_EMP_ID + @"'
    AND ELR_LOAN_LOAN_TYPE = '" + ess.ELR_LOAN_LOAN_TYPE + @"'
    and ELR_REQ_STATUS_ID= 3
").ToInt() > 0)
            {
                Response.Write(APIResult<string>.ErrorResponse("Error: You have already applied '" + (objDB.execute_scalar(@"SELECT  PLTM_LOAN_TYPE_DESC FROM PPM_LOAN_TYPE_MASTER where PLTM_LOAN_TYPE_CODE='" + ess.ELR_LOAN_LOAN_TYPE + @"' ")) + @"' and it is waiting for approval.").GetResponse());
                return;
            }

            string param1 = "";
            if (!LoanEligibility(out param1, ess.ELR_LOAN_LOAN_TYPE, ess.ELR_LOAN_START_DATE))
            {
                if (errorText != "")
                    Response.Write(APIResult<string>.ErrorResponse("Error: " + errorText).GetResponse());
                else
                    Response.Write(APIResult<string>.ErrorResponse("Error: you are not eligible for loan. kindly contact admin").GetResponse());

                return;
            }


            List<ERP1.Pro_Parameters> listOfParams = new List<ERP1.Pro_Parameters>();
            listOfParams.Add(new ERP1.Pro_Parameters { _par_name = "P_LOAN_TYPE", _par_value = ess.ELR_LOAN_LOAN_TYPE });
            listOfParams.Add(new ERP1.Pro_Parameters { _par_name = "P_REF_NO", _par_value = "", _par_in_out = "OUT", _par_data_type = "Varchar2" });

            db.executeProcedure("PAY_GET_LOAN_REF_NUMBER", listOfParams);

            ess.ELR_LOAN_REFERENCE = listOfParams[1]._par_value.ToString();

            //ess.ELR_LOAN_REFERENCE = Request.Form["ELR_LOAN_REFERENCE"];

            if (essLeaveRequestDA.Apply("", false, "01"))
            {
                Response.Write(APIResult<string>.SuccessResponse("The request is sent.").GetResponse());
            }
            else
            {
                Response.Write(APIResult<string>.ErrorResponse("Unable to send the request.").GetResponse());
            }

        }

        private void GET_ANN_FILES()
        {
            var annId = Request["AnnId"];

            DataTable dtfdata = new DataTable("DataTable1");
            dtfdata.Columns.Add("FileName");
            dtfdata.Columns.Add("RelativePath");
            try
            {
                var strpath = Server.MapPath("~/ESS/Docs/Announcement/" + annId.ToString());
                var relativePath = ResolveUrl(@"~/ESS/Docs/Announcement/" + annId.ToString());

                if (Directory.Exists(strpath))
                {
                    string[] filenames = Directory.GetFiles(strpath);

                    for (int i = 0; i < filenames.Length; i++)
                    {
                        //string str = Path.GetFileName(file);
                        string str = new FileInfo(filenames[i]).Name;
                        DataRow dr = dtfdata.NewRow();
                        dr["FileName"] = str;
                        dr["RelativePath"] = relativePath + "/" + str;

                        dtfdata.Rows.Add(dr);
                    }

                }

            }
            catch (Exception ex)
            {
                CommonTasks.WarningMessage(ex.Message, Page);
            }
            var JSONresult = JsonConvert.SerializeObject(dtfdata);
            Response.Write(JSONresult);


        }

        void GET_EMP_ON_LEAVE_TODAY()
        {

            Dictionary<string, object> keyValueParamters = new Dictionary<string, object>();

            string ontcCondition1 = "";
            if (db.execute_scalar(@"SELECT  TAP_PARAVALUE FROM TAS_ATT_PARAM WHERE TAP_PARANAME='APPLY_FILT_LV_CALEN'") == "Y")
            {
                if (ERP.TAS.TASSetUpParameters.GetSetupParameters.IsESSEnabled &&
               (!(ERPCurrentUserInfo.GetCurrentUserInfo.IsAdministrator
             || user.CanBrowseAllEmployees))
               )
                {
                    var eRPCurrentUserInfo = ERPCurrentUserInfo.GetCurrentUserInfo;
                    var db1 = new dbaccess();
                    List<string> arrayQueries = new List<string>();


                    if (eRPCurrentUserInfo.IsBranchAdministrator)
                    {
                        arrayQueries.Add(" select pemp_emp_code from ppm_employee_Details where   PEMP_EMP_BRANCH_CODE=:BranchCode  ");
                        keyValueParamters.Add("BranchCode", eRPCurrentUserInfo.BranchCode);
                    }
                    if (eRPCurrentUserInfo.IsDepartmentHead)
                    {
                        arrayQueries.Add(" select pemp_emp_code from ppm_employee_Details where   PEMP_EMP_DEPTARTMENT_CODE=:DepartmentCode   ");
                        keyValueParamters.Add("DepartmentCode", eRPCurrentUserInfo.DepartmentCode);

                    }

                    arrayQueries.Add(" select pemp_emp_code from ppm_employee_Details where   pemp_reporting_to= :EMP_ID ");
                    arrayQueries.Add(@"SELECT EAFO_EMP_CODE  FROM ESS_APPLY_FOR_OTHERS where EAFO_MANAGER_CODE = :EMP_ID ");
                    keyValueParamters.Add("EMP_ID", eRPCurrentUserInfo.EMP_ID);

                    ontcCondition1 = @" and ( exists ( select 1 from (  Select pemp_emp_code ecode1 from( " + string.Join(" Union All ", arrayQueries) + " ) ) where ecode1= ped.EMPID )  or ped.EMPID=:EMP_ID)";


                }
            }
            var query = @"
SELECT 
    ELR_REQ_STATUS_ID, ELR_SERVICE_TYPE, ELR_EMP_ID, 
   ELR_LEAVE_ID, TO_CHAR(ELR_FROM_DT,'DD/MM/YYYY') ELR_FROM_DT, TO_CHAR(ELR_TO_DT,'DD/MM/YYYY')  ELR_TO_DT, 
   ELR_TOT_DAYS, PLTM_LEAVE_TYPE_CODE, PLTM_LEAVE_TYPE_DESC, 
   EMPID, ENAME, 
   BRANCH_CODE, 
   BRANCH_NAME, DEPT_NAME,
   GRP_ID, GRP_NAME,    
   DESIGNATION_DESC   
FROM ESS_CURRENT_LEAVE ped
where PEMP_EMP_ACTIVE = 'Y'
 
    " + ontcCondition1 + @"
 AND ELR_REQ_STATUS_ID IN (7, 8)
          AND TRUNC (SYSDATE) BETWEEN ELR_FROM_DT AND ELR_TO_DT


";
            LogError.WriteToLogFile("queryqueryquery", false, query);

            var dtLeaves = db.execute_query_retun_datatable(query, keyValueParamters);


            var JSONresult = JsonConvert.SerializeObject(dtLeaves);
            Response.Write(JSONresult);

        }

        void EXITCANCELLATION_REQ()
        {
            bool isLeaveAttchMandotry = false;
            essLeaveRequestDA = new EssLeaveRequestDA();
            essLeaveRequestHeader.LeaveReqDetails.Add(new EssLeaveRequest
            {
                ELR_LEAVE_REQ_ID = (essLeaveRequestHeader.LeaveReqDetails.Count() > 0 ? essLeaveRequestHeader.LeaveReqDetails.Max(m => m.ELR_LEAVE_REQ_ID) + 1 : 1),
                ELR_SERVICE_TYPE = ESSServices.ResignationCancellation
            });

            EssLeaveRequest ess = essLeaveRequestHeader.LeaveReqDetails[0];
            ess.ELR_REQUESTED_DT = Request.Form["ESSfromDate"].ToArabicDate();
            ess.ELR_LEAVE_ID = "";
            ess.ELR_FROM_DT = Request.Form["ESSfromDate"].ToArabicDate();
            ess.ELR_TO_DT = Request.Form["ESSfromDate"].ToArabicDate();
            ess.ELR_SUBJ = "";
            ess.AttendacneOtherReasonDesc = ess.ELR_BODY = Request.Form["leavedesc"]; ; // = "Letter Request";
            ess.AttendacneReasonCode = "";




            ess.ELR_EMP_ID = Request.Form["EMPCode"];
            ess.ELR_LETTER_TYPE = "";
            ess.ELR_LETTER_TO_BANK = "";
            //ess.ELR_LETTER_YYYYMM = "";

            essLeaveRequestHeader.ELR_FROM_DT = ess.ELR_FROM_DT;
            essLeaveRequestHeader.ELR_TO_DT = ess.ELR_TO_DT;
            essLeaveRequestHeader.ELR_LEAVE_ID = ess.ELR_LEAVE_ID;
            essLeaveRequestHeader.ELR_SUBJ = ess.ELR_SUBJ;
            essLeaveRequestHeader.ELR_BODY = ess.ELR_BODY;

            essLeaveRequestHeader.ELR_EMP_ID = ess.ELR_EMP_ID;


            essLeaveRequestHeader.ELR_SERVICE_TYPE = ESSServices.ResignationCancellation;
            ess.ELR_SERVICE_TYPE = ESSServices.ResignationCancellation;
            //CountryName

            essLeaveRequestHeader.ELR_CREATED_USER_ID = Request.Form["UserId"];
            ess.ELR_LOAN_REFERENCE = "";


            //.ELR_CREATED_USER_ID = Request.Form["UserId"];
            if (ess.ELR_FROM_DT == null || ess.ELR_TO_DT == null)
            {
                Response.Write(APIResult<string>.ErrorResponse("Error: Enter the from date and to date values").GetResponse());
                return;
            }
            if (ess.ELR_FROM_DT.Value > ess.ELR_TO_DT.Value)
            {
                Response.Write(APIResult<string>.ErrorResponse("Error: From date should be greater than to date.").GetResponse());
                return;
            }

            int totRecordscnt = objDB.record_found(@"
            SELECT COUNT(*) FROM ESS_LEAVE_REQ 
            WHERE ELR_SERVICE_TYPE = 12
              AND  ELR_EMP_ID='" + ess.ELR_EMP_ID + @"' AND ELR_REQ_STATUS_ID IN(7, 8)   
            ");

            if (totRecordscnt == 0)
            {
                Response.Write(APIResult<string>.ErrorResponse("Error: You are not approval the Resignation.").GetResponse());
                return;
            }

            //ELR_LETTER_TO_BANK, ELR_LETTER_YYYYMM,ELR_LETTER_TYPE and ELR_REQUESTED_DT='" + ess.ELR_REQUESTED_DT + @"'
            var From_Dt = DateTime.Now.ToString("dd/MM/yyyy");
            int totRecordsPending = objDB.record_found(@"SELECT COUNT(*) FROM ESS_LEAVE_REQ 
            WHERE ELR_SERVICE_TYPE = 29
                 AND  ELR_EMP_ID='" + ess.ELR_EMP_ID + @"'  AND ELR_REQ_STATUS_ID   IN  ('3','4') ");



            if (totRecordsPending > 0)
            {
                Response.Write(APIResult<string>.ErrorResponse("Error: Previous  request is pending.").GetResponse());
                return;
            }

            //if (ess.ELR_SUBJ.Length > 499)
            //{
            //    Response.Write("Error: Subject Total characters cant be more than 500.");
            //    return;
            //}

            if (ess.ELR_BODY.Length > 1499)
            {
                Response.Write(APIResult<string>.ErrorResponse("Error: Reason for Cancellation Total characters cant be more than 2000.").GetResponse());
                return;
            }

            //            var isLeaveAttchMandotry = db.execute_scalar(@" SELECT 
            //     nvl(PLTM_ATTH_MANDATORY,'N') 
            //FROM PPM_LEAVE_TYPE_MASTER  where PLTM_LEAVE_TYPE_CODE='" + ess.ELR_LEAVE_ID + @"' ").Equals("Y");

            //            AppCode.LogError.WriteToLogFile("isLeaveAttchMandotry", false, isLeaveAttchMandotry.ToString());

            if (isLeaveAttchMandotry)
            {
                var oldFolderPath = HttpContext.Current.Request.MapPath("~/ESS/Docs/") + "/" + ess.ELR_EMP_ID + "/"
               + "/" + (int)ess.ELR_SERVICE_TYPE + "/" + HttpContext.Current.Session.SessionID;
                var filesAttached = false;
                if (Directory.Exists(oldFolderPath))
                {
                    HttpPostedFile file = HttpContext.Current.Request.Files["fileInput"];

                    var files = Directory.GetFiles(oldFolderPath);

                    if (file != null && file.ContentLength > 0)
                    {
                        filesAttached = true;
                    }
                }

                if (filesAttached == false)
                {
                    ResponseWrite("Error: attachment is mandatory.");
                    return;
                }
            }
            if (essLeaveRequestDA.Apply("", false, "01"))
            {
                GENERALSaveDocs("29");
                WriteSuccessMessage("The request is sent.");
                objDB.execute_query(@"UPDATE ESS_LEAVE_REQ SET ELR_REQ_STATUS_ID=11 WHERE ELR_SERVICE_TYPE = 12
                 AND  ELR_EMP_ID='" + ess.ELR_EMP_ID + @"'  AND ELR_REQ_STATUS_ID   IN  ('8') ");
            }
            else
            {
                ResponseWrite("Unable to send the request.");
            }
        }

        void GENERALApproverDocs(string serviceType, string requestid)
        {
            essLeaveRequestDA = new EssLeaveRequestDA(requestid.ToInt());
            long leaveReqHeaderID = 0;
            leaveReqHeaderID = essLeaveRequestDA.leaveReqHeader.ELR_LEAVE_REQ_HEAD_ID.ToString().ToInt();
            string Empcode = essLeaveRequestDA.leaveReqHeader.ELR_EMP_ID.ToString();

            try
            {
                Directory.CreateDirectory(Server.MapPath("~/ESS/Docs"));
            }
            catch { }
            try
            {
                Directory.CreateDirectory(Server.MapPath("~/ESS/Docs/" + Empcode));
            }
            catch { }

            try
            {
                Directory.CreateDirectory(Server.MapPath("~/ESS/Docs/" + Empcode + "/" + serviceType));
            }
            catch { }


            try
            {
                Directory.CreateDirectory(Server.MapPath("~/ESS/Docs/" + Empcode + "/" + serviceType + "/" + leaveReqHeaderID));

            }
            catch { }

            try
            {
                Directory.CreateDirectory(Server.MapPath("~/ESS/Docs/" + Empcode + "/" + serviceType + "/" + leaveReqHeaderID + "/" + "HrDocs"));

            }
            catch { }

            HttpPostedFile file = HttpContext.Current.Request.Files["fileInputHr"];

            try
            {
                string[] Files = Directory.GetFiles(Server.MapPath("~/ESS/Docs/" + Empcode + "/" + serviceType + "/" + leaveReqHeaderID + "/" + "HrDocs"));
                foreach (string fi in Files)
                {
                    File.Delete(fi);
                }


                if (file != null && file.ContentLength > 0)
                {
                    string fname = Path.GetExtension(file.FileName);

                    file.SaveAs(Server.MapPath("~/ESS/Docs/" + Empcode + "/" + serviceType + "/" + leaveReqHeaderID + "/" + "HrDocs/" + file.FileName));


                }


            }
            catch (Exception e1)
            {
                AppCode.LogError.WriteToLogFile(e1.ToString(), "", "");
            }
            HttpPostedFile file2 = Request.Files["fileInputArabic"];

            //check file was submitted
            if (file2 != null && file2.ContentLength > 0)
            {
                string fname = Path.GetExtension(file2.FileName);
                file2.SaveAs(Server.MapPath("~/ESS/Docs/" + Empcode + "/" + serviceType + "/" + leaveReqHeaderID + "/" + "HrDocs" + file2.FileName));
                //file2.SaveAs(Server.MapPath(Path.Combine("~/App_Data/", fname)));
            }




        }
        void LOAD_EXIT_CANCEL_HISTORY(string empCode)
        {
            var Services = Request.Form["Service"];
            var fromDate1 = Request.QueryString["FromDate"].ToArabicDate(); ;
            var toDate1 = Request.QueryString["ToDate"].ToArabicDate();

            string strquery = @"SELECT 
     ESS_LEAVE_REQ_DETAILS_ALL.*,
     TO_CHAR(ESS_LEAVE_REQ_DETAILS_ALL.ELRH_REQUESTED_DT,'DD-Mon-YYYY','nls_date_language=english') DT1,       
          APPROVED_GROUP_NAME 
          CURR_STATUS_NAME_1,to_char(ELRH_REQUESTED_DT,'dd/mm/yyyy') REQUESTED_DT,APP_CNT, CONCAT(CONCAT(APP_DATE,' '),APP_TIME)APP_DATE,APP_TIME,ELRH_REQ_STATUS_ID
   
FROM 
    ESS_LEAVE_REQ_DETAILS_ALL,
        ESS_REQ_APPROVED_GROUP,
      ( select AWDD_WF_HEADER_ROW_ID WF_HEADER_ID, SUM(CASE WHEN wd2.AWDD_STATUS in ('A','R') THEN 1 ELSE 0 END) AS APP_CNT
  from AUTH_WF_DOC_DETAIL wd2  
                    GROUP BY AWDD_WF_HEADER_ROW_ID)
    where
             UNIQUE_ID_1 =  TO_CHAR(LEAVE_REQ_ID)
                    AND UNIQUE_ID_2 = TO_CHAR(LEAVE_REQ_ID) 
    and WF_HEADER_ID(+) = AWH_ROW_ID1
    AND EST_SERVICE_ID =29 
            AND EMP_ID = '" + empCode + @"'
             and((FROM_DT BETWEEN to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy') OR

           TO_DT BETWEEN to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy')) OR
               (FROM_DT < to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy') < TO_DT))

ORDER BY LEAVE_REQ_ID DESC ";

            // and EST_SERVICE_ID = " + Services + @"

            DataTable dt1 = objDB.execute_query_retun_datatable(strquery);
            if (dt1 != null && dt1.Rows.Count > 0)
            {
                var JSONresult = JsonConvert.SerializeObject(dt1);
                Response.Write(JSONresult);
            }
            else
            {
                Response.Write("{}");
            }


        }

        void LOAD_EXIT_CANCEL_APPROVED(string empCode)
        {

            var from = Request["FromDate"].ToArabicDate();
            var to = Request["ToDate"].ToArabicDate();
            var Services = Request.Form["Service"];
            DBAccess ObjDb = new DBAccess();
            // var streval = objDB.execute_scalar("SELECT TAP_PARAVALUE FROM  TAS_ATT_PARAM WHERE  TAP_PARANAME='COMP_OFF_LEAVE_CODE' ");
            string strquery = @"

SELECT 
     ESS_LEAVE_REQ_DETAILS_ALL.*,
      
          APPROVED_GROUP_NAME 
          CURR_STATUS_NAME_1,to_char(ELRH_REQUESTED_DT,'dd/mm/yyyy') REQUESTED_DT, CONCAT(CONCAT(APP_DATE,' '),APP_TIME)APP_DATE,APP_TIME
    
   
FROM 
     ESS_LEAVE_REQ_DETAILS_ALL,
        ESS_REQ_APPROVED_GROUP,
      ( select AWDD_WF_HEADER_ROW_ID WF_HEADER_ID, SUM(CASE WHEN wd2.AWDD_STATUS in ('A','R') THEN 1 ELSE 0 END) AS APP_CNT
  from AUTH_WF_DOC_DETAIL wd2  
                    GROUP BY AWDD_WF_HEADER_ROW_ID)
    where
             UNIQUE_ID_1 =  TO_CHAR(LEAVE_REQ_ID)
                    AND UNIQUE_ID_2 = TO_CHAR(LEAVE_REQ_ID) 
    and WF_HEADER_ID(+) = AWH_ROW_ID1
    AND EST_SERVICE_ID =29     

and exists ( 
    SELECT 1 FROM AUTH_WF_HEADER, AUTH_WF_DOC_DETAIL
              WHERE     
                    AWDD_WF_HEADER_ROW_ID = AWH_ROW_ID
                    AND AWDD_EMP_APPROVED_BY = '" + empCode + @"'
                    AND AWH_UNIQUE_ID1 =  TO_CHAR(LEAVE_REQ_ID)
                    AND AWH_UNIQUE_ID2 = TO_CHAR(LEAVE_REQ_ID)
    )

             and((FROM_DT BETWEEN to_date('" + from.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy') AND to_date('" + to.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy') OR
            TO_DT BETWEEN to_date('" + from.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy') AND to_date('" + to.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy')) OR
                (FROM_DT < to_date('" + from.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy') AND to_date('" + to.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy') < TO_DT))
ORDER BY LEAVE_REQ_ID DESC";

            DataTable dt1 = objDB.execute_query_retun_datatable(strquery);
            if (dt1 != null && dt1.Rows.Count > 0)
            {
                var JSONresult = JsonConvert.SerializeObject(dt1);
                Response.Write(JSONresult);
            }
            else
            {
                Response.Write("{}");
            }
        }


        void Exit_Cancel_Approve()
        {

            List<string> queries = new List<string>();
            var reqHead = Request.Form["ReqId"];
            ESSWorkflowHeader ESSwORKFLOW = new ESSWorkflowHeader(reqHead.ToString(), reqHead.ToString());

            var strquery = "SELECT FUN_ESS_APVL_CMNT_STG(" + reqHead + ",'APPROVE') FROM DUAL";
            var strcomm = objDB.execute_scalar(strquery);
            if (strcomm == "YES" && Request.Form["approveComment"].Trim() == "")
            {
                Response.Write(APIResult<string>.ErrorResponse("Error: Enter Comments.").GetResponse());
                return;
            }

            var comments = Request.Form["approveComment"];

            var orgCost = "";

            if (comments.Length > 1400)
            {
                Response.Write(APIResult<string>.ErrorResponse("Error: Comments cant exceed 1400 characters.").GetResponse());
                return;
            }
            if (!string.IsNullOrEmpty(Request["EARD_ORG_SOFTWARE_COST"]))
            {
                orgCost = Request["EARD_ORG_SOFTWARE_COST"].ToString();

                objDB.execute_query(@"
UPDATE ESS_AER_REQUEST_DET
SET    
       EARD_ORG_SOFTWARE_COST    = '" + orgCost + @"'
WHERE  EARD_REQUEST_HEAD_ID      = '" + reqHead + @"'
");

            }
            // adde by bala on 13-06-2023 , approver cant approve again and again by clicking same approve button
            if (!IsApproverFoundInWorkflow(reqHead.ToInt()))
                return;
            if (ESSwORKFLOW.UpdateWFStatus(true, comments))
            {
                var Refid = db.execute_scalar("select LQ.ELR_LEAVE_REQ_HEAD_ID from ESS_LEAVE_REQ LQ WHERE LQ.ELR_SERVICE_TYPE=12 AND ELR_REQ_STATUS_ID IN(7,8) AND EXISTS(SELECT * FROM ESS_LEAVE_REQ WHERE ELR_SERVICE_TYPE=29 AND ELR_REQ_STATUS_ID IN(7,8) AND ELR_LEAVE_REQ_HEAD_ID=" + reqHead + " AND LQ.ELR_EMP_ID=ELR_EMP_ID)");

                queries.Add(@"UPDATE ESS_LEAVE_REQ_HEAD SET ELRH_REQ_STATUS_ID =11 WHERE ELRH_LEAVE_REQ_ID   =" + Refid + "");
                queries.Add(@"UPDATE ESS_LEAVE_REQ SET ELR_REQ_STATUS_ID =11 WHERE ELR_LEAVE_REQ_HEAD_ID   =" + Refid + "");

                db.executeQueryInTrasnaction(queries);

                Response.Write(APIResult<string>.SuccessResponse("The record is approved.").GetResponse());
            }
            else
                Response.Write(APIResult<string>.ErrorResponse("Error: Unable to approve the record.").GetResponse());

        }


        void SaveTerminationOrResignation()
        {


            essLeaveRequestDA = new EssLeaveRequestDA();
            essLeaveRequestHeader.LeaveReqDetails.Add(new EssLeaveRequest
            {
                ELR_LEAVE_REQ_ID = (essLeaveRequestHeader.LeaveReqDetails.Count() > 0 ? essLeaveRequestHeader.LeaveReqDetails.Max(m
                => m.ELR_LEAVE_REQ_ID) + 1 : 1),
                ELR_SERVICE_TYPE = ESSServices.ResignationRequest
            });
 

            EssLeaveRequest ess = essLeaveRequestHeader.LeaveReqDetails[0];

            ess.ELR_LEAVE_ID = "RE";

            essLeaveRequestHeader.ELR_SERVICE_TYPE = ess.ELR_SERVICE_TYPE = ESSServices.ResignationRequest;

            essLeaveRequestHeader.ELR_EMP_ID = ess.ELR_EMP_ID = user.Emp_Code;

            //HTH_TYPE_OF_TERMINATION, HTH_REASON_FOR_TERMINATION, HTH_DATE_OF_RESIGANTION, 
            //HTH_NOTICE_PERIOD_IN_DAYS, HTH_LAST_WORKING_DATE, HTH_COMMENTS, HTH_TERM_CONDITIONS

            essLeaveRequestHeader.ELR_FROM_DT = ess.ELR_FROM_DT = DateTime.Now.Date;
            essLeaveRequestHeader.ELR_TO_DT = ess.ELR_TO_DT = DateTime.Now.Date;
            //essLeaveRequestHeader.ELR_TO_DT = ess.ELR_TO_DT = txtTravelEndDate.Text.ToArabicDate();

            essLeaveRequestHeader.ELR_SUBJ = ess.ELR_SUBJ = "Resignation Request";
            essLeaveRequestHeader.ELR_BODY = ess.ELR_BODY = "Resignation request on " + Request["HTH_DATE_OF_RESIGANTION"];
            //essLeaveRequestHeader.AttendacneOtherReasonDesc ess.AttendacneOtherReasonDesc = txtTourAgenda.Text;
            ess.AttendacneReasonCode = "";


            essLeaveRequestHeader.ELR_CREATED_USER_ID = ess.ELR_CREATED_USER_ID = user.User_Id.ToString();
            essLeaveRequestHeader.ELR_REQUESTED_DT = ess.ELR_REQUESTED_DT = DateTime.Now.Date;

            // check resignation alreadu applied
            if (string.IsNullOrEmpty(Request["HTH_DATE_OF_RESIGANTION"]))
            {
                WriteErrorMessage("Error: Date of resignation cant be empty.");
                return;
            }

            //SELECT ELR_EMP_ID, ELR_SERVICE_TYPE, ELR_REF_DOC_NO = HTH_DOCUMENT_NO FROM ESS_LEAVE_REQ

            if (db.record_found(@"SELECT Count(1) FROM ESS_LEAVE_REQ where ELR_SERVICE_TYPE ='12' AND   ELR_REQ_STATUS_ID   IN ( 3,4)    and  ELR_EMP_ID ='" + user.Emp_Code + @"' ") > 0)
            {
                WriteErrorMessage("Error:Previous resignation request is pending");
                return;
            }
            //HTH_DOCUMENT_NO
            if (db.record_found(@"SELECT Count(1) FROM ESS_LEAVE_REQ where ELR_SERVICE_TYPE ='12' AND   ELR_REQ_STATUS_ID   IN ( 7,8)    and  ELR_EMP_ID ='" + user.Emp_Code + @"' ") > 0)
            {
                WriteErrorMessage("Error: you have already submitted the resignation request");
                return;
            }



 
            EmpExitRequest ObjEmpExitReq = new EmpExitRequest();
            //HTH_TYPE_OF_TERMINATION, HTH_REASON_FOR_TERMINATION, HTH_DATE_OF_RESIGANTION, 
            //HTH_NOTICE_PERIOD_IN_DAYS, HTH_LAST_WORKING_DATE, HTH_COMMENTS, HTH_TERM_CONDITIONS

            ObjEmpExitReq.DocSysId = 90;
            ObjEmpExitReq.DocDate = "";
            ObjEmpExitReq.EmpCode = user.Emp_Code;
            ObjEmpExitReq.DocDate = DateTime.Now.Date.ToString("dd/MM/yyyy");

            ObjEmpExitReq.TypeOfResingation = Request.QueryString["Termination"];
            ObjEmpExitReq.ReasonforResignation = Request["HTH_REASON_FOR_TERMINATION"];
            ObjEmpExitReq.DateOfResignation = Request["HTH_DATE_OF_RESIGANTION"];
            ObjEmpExitReq.NoticePeriodDays = Request["HTH_NOTICE_PERIOD_IN_DAYS"].ToInt();
            ObjEmpExitReq.LastWorkingDate = Request["HTH_LAST_WORKING_DATE"];
            ObjEmpExitReq.EmployeeComments = Request["HTH_COMMENTS"].ToString().Replace("'", "''");
            ObjEmpExitReq.TermsAndCondition = Request["HTH_TERM_CONDITIONS"].ToString().Replace("'", "''");
            ObjEmpExitReq.IsTermsAndConditionAccepted = true;
            ObjEmpExitReq.RequestStatus = "P";
            ObjEmpExitReq.CreatedUser = user.User_Id.ToString();
            //ObjEmpExitReq.WorkFlowStatus = "NA";             

            if (ObjEmpExitReq.EmployeeComments.Length > 999)
            {
                WriteErrorMessage("Error: Comments cant exceed more than 1000 characters");
                return;
            }
            //txtDateOfResignation.Text.ToArabicDate() < txtHireDate.SelectedDate
            if (db.record_found(@"
            Select Count(1) 
    From PPM_EMPLOYEE_DETAILS where pemp_emp_code='" + user.Emp_Code + @"' 
    and  PEMP_EMP_JOIN_DATE >= to_date('" + Request["HTH_DATE_OF_RESIGANTION"] + @"','dd/mm/yyyy') ") > 0)
            {
                WriteErrorMessage("Error: Resignation Date cannot be less than Employee Hire Date");
                return;
            }

            ess.ELR_TOT_DAYS = essLeaveRequestHeader.ELR_TOT_DAYS = (ess.ELR_TO_DT.Value - ess.ELR_FROM_DT.Value).TotalDays;
            if (ObjEmpExitReq.Save())
            {
                ess.RefDocumentCode = ObjEmpExitReq.DocNumber;
                if (essLeaveRequestDA.Apply("", false, "01"))
                {
                    WriteSuccessMessage("The request is sent.");
                }
                else
                {
                    WriteErrorMessage("Error: Error while applying request.");
                }
            }

            else
            {
                WriteErrorMessage("Error: Unable to send the request.");
            }
             
        }

        void SaveTermination()
        {


            essLeaveRequestDA = new EssLeaveRequestDA();
            essLeaveRequestHeader.LeaveReqDetails.Add(new EssLeaveRequest
            {
                ELR_LEAVE_REQ_ID = (essLeaveRequestHeader.LeaveReqDetails.Count() > 0 ? essLeaveRequestHeader.LeaveReqDetails.Max(m
                => m.ELR_LEAVE_REQ_ID) + 1 : 1),
                ELR_SERVICE_TYPE = ESSServices.Termination
            });




            EssLeaveRequest ess = essLeaveRequestHeader.LeaveReqDetails[0];

            ess.ELR_LEAVE_ID = "";

            essLeaveRequestHeader.ELR_SERVICE_TYPE = ess.ELR_SERVICE_TYPE = ESSServices.Termination;

            essLeaveRequestHeader.ELR_EMP_ID = ess.ELR_EMP_ID = Request["empActing"];


            essLeaveRequestHeader.ELR_FROM_DT = ess.ELR_FROM_DT = DateTime.Now.Date;
            essLeaveRequestHeader.ELR_TO_DT = ess.ELR_TO_DT = DateTime.Now.Date;
            //essLeaveRequestHeader.ELR_TO_DT = ess.ELR_TO_DT = txtTravelEndDate.Text.ToArabicDate();

            essLeaveRequestHeader.ELR_SUBJ = ess.ELR_SUBJ = "Termination Request";
            essLeaveRequestHeader.ELR_BODY = ess.ELR_BODY = "Termination request on " + Request["HTH_DATE_OF_RESIGANTION"];
            //essLeaveRequestHeader.AttendacneOtherReasonDesc ess.AttendacneOtherReasonDesc = txtTourAgenda.Text;
            ess.AttendacneReasonCode = "";


            essLeaveRequestHeader.ELR_CREATED_USER_ID = ess.ELR_CREATED_USER_ID = user.User_Id.ToString();
            essLeaveRequestHeader.ELR_REQUESTED_DT = ess.ELR_REQUESTED_DT = DateTime.Now.Date;

            // check resignation alreadu applied
            if (string.IsNullOrEmpty(Request["HTH_DATE_OF_RESIGANTION"]))
            {
                WriteErrorMessage("Error: Date of Termination cant be empty.");
                return;
            }

            //SELECT ELR_EMP_ID, ELR_SERVICE_TYPE, ELR_REF_DOC_NO = HTH_DOCUMENT_NO FROM ESS_LEAVE_REQ

            if (db.record_found(@"SELECT Count(1) FROM ESS_LEAVE_REQ where ELR_SERVICE_TYPE ='30' AND   ELR_REQ_STATUS_ID   IN ( 3,4)    and  ELR_EMP_ID ='" + ess.ELR_EMP_ID + @"' ") > 0)
            {
                WriteErrorMessage("Error:Previous Termination request is pending");
                return;
            }
            //HTH_DOCUMENT_NO
            if (db.record_found(@"SELECT Count(1) FROM ESS_LEAVE_REQ where ELR_SERVICE_TYPE ='30' AND   ELR_REQ_STATUS_ID   IN ( 7,8)    and  ELR_EMP_ID ='" + ess.ELR_EMP_ID + @"' ") > 0)
            {
                WriteErrorMessage("Error: you have already submitted the Termination request");
                return;
            }


            EmpExitRequest ObjEmpExitReq = new EmpExitRequest();
            //HTH_TYPE_OF_TERMINATION, HTH_REASON_FOR_TERMINATION, HTH_DATE_OF_RESIGANTION, 
            //HTH_NOTICE_PERIOD_IN_DAYS, HTH_LAST_WORKING_DATE, HTH_COMMENTS, HTH_TERM_CONDITIONS

            ObjEmpExitReq.DocSysId = 90;
            ObjEmpExitReq.DocDate = "";
            ObjEmpExitReq.EmpCode = ess.ELR_EMP_ID;
            ObjEmpExitReq.DocDate = DateTime.Now.Date.ToString("dd/MM/yyyy");

            ObjEmpExitReq.TypeOfResingation = Request.QueryString["Termination"];
            ObjEmpExitReq.ReasonforResignation = Request["HTH_REASON_FOR_TERMINATION"];
            ObjEmpExitReq.DateOfResignation = Request["HTH_DATE_OF_RESIGANTION"];
            ObjEmpExitReq.NoticePeriodDays = Request["HTH_NOTICE_PERIOD_IN_DAYS"].ToInt();
            ObjEmpExitReq.LastWorkingDate = Request["HTH_LAST_WORKING_DATE"];
            ObjEmpExitReq.EmployeeComments = Request["HTH_COMMENTS"].ToString().Replace("'", "''");
            ObjEmpExitReq.TermsAndCondition = Request["HTH_TERM_CONDITIONS"].ToString().Replace("'", "''");
            ObjEmpExitReq.IsTermsAndConditionAccepted = true;
            ObjEmpExitReq.RequestStatus = "P";
            ObjEmpExitReq.CreatedUser = user.User_Id.ToString();
            //ObjEmpExitReq.WorkFlowStatus = "NA";             

            if (ObjEmpExitReq.EmployeeComments.Length > 999)
            {
                WriteErrorMessage("Error: Comments cant exceed more than 1000 characters");
                return;
            }

            if (db.record_found(@"
            Select Count(1) 
    From PPM_EMPLOYEE_DETAILS where pemp_emp_code='" + user.Emp_Code + @"' 
    and  PEMP_EMP_JOIN_DATE >= to_date('" + Request["HTH_DATE_OF_RESIGANTION"] + @"','dd/mm/yyyy') ") > 0)
            {
                WriteErrorMessage("Error: Termination Date cannot be less than Employee Hire Date");
                return;
            }

            ess.ELR_TOT_DAYS = essLeaveRequestHeader.ELR_TOT_DAYS = (ess.ELR_TO_DT.Value - ess.ELR_FROM_DT.Value).TotalDays;
            if (ObjEmpExitReq.Save())
            {
                ess.RefDocumentCode = ObjEmpExitReq.DocNumber;
                if (essLeaveRequestDA.Apply("", false, "01"))
                {
                    WriteSuccessMessage("The request is sent.");
                }
                else
                {
                    WriteErrorMessage("Error: Error while applying request.");
                }
            }

            else
            {
                WriteErrorMessage("Error: Unable to send the request.");
            }


        }


        void SaveExitFormn()
        {


            essLeaveRequestDA = new EssLeaveRequestDA();
            essLeaveRequestHeader.LeaveReqDetails.Add(new EssLeaveRequest
            {
                ELR_LEAVE_REQ_ID = (essLeaveRequestHeader.LeaveReqDetails.Count() > 0 ? essLeaveRequestHeader.LeaveReqDetails.Max(m
                => m.ELR_LEAVE_REQ_ID) + 1 : 1),
                ELR_SERVICE_TYPE = ESSServices.ExitForm
            });


            EssLeaveRequest ess = essLeaveRequestHeader.LeaveReqDetails[0];

          

            essLeaveRequestHeader.ELR_SERVICE_TYPE = ess.ELR_SERVICE_TYPE = ESSServices.ExitForm;

            essLeaveRequestHeader.ELR_EMP_ID = ess.ELR_EMP_ID = user.Emp_Code;

             

            essLeaveRequestHeader.ELR_FROM_DT = ess.ELR_FROM_DT = DateTime.Now.Date;
            essLeaveRequestHeader.ELR_TO_DT = ess.ELR_TO_DT = DateTime.Now.Date;
          

            essLeaveRequestHeader.ELR_SUBJ = ess.ELR_SUBJ = Request["HTH_COMMENTS"];
            essLeaveRequestHeader.ELR_BODY = ess.ELR_BODY =  Request["HTH_COMMENTS"];
            //essLeaveRequestHeader.AttendacneOtherReasonDesc ess.AttendacneOtherReasonDesc = txtTourAgenda.Text;
            ess.AttendacneReasonCode = "";


            essLeaveRequestHeader.ELR_CREATED_USER_ID = ess.ELR_CREATED_USER_ID = user.User_Id.ToString();
            essLeaveRequestHeader.ELR_REQUESTED_DT = ess.ELR_REQUESTED_DT = DateTime.Now.Date;

            // check resignation alreadu applied
            if (!string.IsNullOrEmpty(Request["HTH_COMMENTS"]))
            {
                if (Request["HTH_COMMENTS"].Length > 500)
                {
                    WriteErrorMessage("Error:  Comments cant exceed more than 500 characters.");
                    return;
                }
            }

			//SELECT ELR_EMP_ID, ELR_SERVICE_TYPE, ELR_REF_DOC_NO = HTH_DOCUMENT_NO FROM ESS_LEAVE_REQ

			if (db.record_found(@"SELECT Count(1) FROM ESS_LEAVE_REQ where ELR_SERVICE_TYPE ='39' AND   ELR_REQ_STATUS_ID   IN (3)    and  ELR_EMP_ID ='" + user.Emp_Code + @"' ") > 0)
			{
				WriteErrorMessage("Error:Previous Exit Form is pending");
				return;
			}

            //if (db.record_found(@"SELECT Count(1) FROM ESS_LEAVE_REQ where ELR_SERVICE_TYPE ='39' AND   ELR_REQ_STATUS_ID   IN (7,8)    and  ELR_EMP_ID ='" + user.Emp_Code + @"' ") > 0)
            //{
            //    WriteErrorMessage("Error:Aleady exist this Exit Form ");
            //    return;
            //}


            if (essLeaveRequestDA.Apply("", false, "01"))
        {
                SaveDocs("39", user.Emp_Code);
            WriteSuccessMessage("The request is sent.");
        }
        else
        {
            WriteErrorMessage("Error: Error while applying request.");
        }
            

             

        }

        private void GET_REASON_FOR_RESIGNATION()
        {
            DBAccess db = new DBAccess();

            var strResigType = Request["RESIGNATION_TYPE"];

            //var resignDate = db.execute_query_retun_datatable(@"");
            var strSelectreasonQuery = @"";


            if (strResigType == "V")
                strSelectreasonQuery = @"SELECT TRIM(TRM_REASON_DESC)  TRM_REASON_DESC FROM TAS_REASONS_MASTER WHERE TRM_TRANS_TYPE = 'EXV'";
            else if (strResigType == "I")
                strSelectreasonQuery = @"SELECT TRIM(TRM_REASON_DESC) TRM_REASON_DESC FROM TAS_REASONS_MASTER WHERE TRM_TRANS_TYPE = 'EXI'";
            //TRM_REASON_DESC

            var dt1 = db.execute_query_retun_datatable(strSelectreasonQuery);

            var JSONresult = JsonConvert.SerializeObject(dt1);
            Response.Write(JSONresult);
            // Response.Write(resignDate.ToString("dd/MM/yyyy"));

        }
        private void GET_RESIGNATION_DATE()
        {
            DBAccess db = new DBAccess();

            var HTH_DATE_OF_RESIGANTION = Request["HTH_DATE_OF_RESIGANTION"].ToArabicDateNull();
            var HTH_NOTICE_PERIOD_IN_DAYS = Request["HTH_NOTICE_PERIOD_IN_DAYS"].ToInt();

            if (HTH_DATE_OF_RESIGANTION == null)
            {
                WriteSuccessMessage("{}");
                return;
            }
            if (HTH_NOTICE_PERIOD_IN_DAYS == 0)
            {
                WriteSuccessMessage(HTH_DATE_OF_RESIGANTION.Value.ToString("dd/MM/yyyy"));
                return;
            }
            var resignDate = HTH_DATE_OF_RESIGANTION.Value.AddDays(HTH_NOTICE_PERIOD_IN_DAYS);

            WriteSuccessMessage(resignDate.ToString("dd/MM/yyyy"));

        }

        private void PERMISSION_SESSION_CNT()
        {
            DBAccess db = new DBAccess();

            var empCode = Request["EmpCode"];
            var attDate = Request["AttDate"].ToArabicDate();
            var tasId = db.execute_scalar(@"SELECT  TU_TAS_ID  FROM TAS_USERINFO where TU_EMP_CODE ='" + empCode + @"' ").ToLong();

            if (tasId <= 0)
            {
                WriteErrorMessage("Error: Tas Id not defined for the user " + empCode);
                return;
            }
            // define att

            TASAttTran.GenerateAttRecords(attDate);
            var q = @"
SELECT 
    TO_CHAR( IN1_S, 'hh:mi AM', 'nls_date_language=english') AS IN1_S, TO_CHAR( OUT1_S, 'hh:mi AM', 'nls_date_language=english') AS OUT1_S,
    TO_CHAR( IN2_S, 'hh:mi AM', 'nls_date_language=english') AS IN2_S, TO_CHAR( OUT2_S, 'hh:mi AM', 'nls_date_language=english') AS OUT2_S,
    TO_CHAR( IN1_A, 'hh:mi AM', 'nls_date_language=english') AS IN1_A, TO_CHAR( OUT1_A, 'hh:mi AM', 'nls_date_language=english') AS OUT1_A,
    TO_CHAR( IN2_A, 'hh:mi AM', 'nls_date_language=english') AS IN2_A, TO_CHAR( OUT2_A, 'hh:mi AM', 'nls_date_language=english') AS OUT2_A,
    LEAVE_TYPE_ID, HOLIDAY_ID, TOT_SHIFT_PUNCH_INCNT
FROM 
    ATT_TRAN 
WHERE 
    TAS_ID='" + tasId + "' AND  ATT_DATE=TO_DATE('" + attDate.ToString("dd/MM/yyyy") + "','dd/MM/yyyy') ";

            var dtsESSIOniNFO = db.execute_query_retun_datatable(q);


            var JSONresult = JsonConvert.SerializeObject(dtsESSIOniNFO);
            Response.Write(JSONresult);

        }

        void LEAVE_TYPE_ACTING_ENABLED_YN()
        {
            var leaveType = Request["LeaveTypeId"];

            var actingEnabled = db.execute_scalar(@"SELECT 	 PLTM_ACTING_MNG_NOTIFY FROM PPM_LEAVE_TYPE_MASTER WHERE PLTM_LEAVE_TYPE_CODE='" + leaveType + @"'");
            if (!string.IsNullOrEmpty(actingEnabled) && actingEnabled == "Y")
            {
                ResponseWrite("Y");
            }
            else
                ResponseWrite("N");

        }
        void ESS_WF_EMP_MAP_SAVE()
        {

            var empCode = Request["EmpCode"];
            var essDocId = Request["EssId"];
            var customWorkflowId = Request["wfId"];

            var recCount = db.record_found(@"Select count(1) from AUTH_WF_EMP_DOC_MAP where AWEDM_ESS_DOC_ID ='" + essDocId
                + @"' and AWEDM_EMP_CODE ='" + empCode + @"'");
            int insertResult = 0;
            //ESS_WF_EMP_MAP_SAVE
            // if no custom workflow selected then delte existing custom workflow for that employee
            if (string.IsNullOrEmpty(customWorkflowId))
            {
                insertResult = db.execute_query(@"
Delete From  AUTH_WF_EMP_DOC_MAP where AWEDM_ESS_DOC_ID ='" + essDocId
                + @"' and AWEDM_EMP_CODE ='" + empCode + @"'" );
                if (insertResult > 0)
                    WriteSuccessMessage("Custom workflow deleted successfully");
                else
                    WriteErrorMessage("No workflow assigned to delete.");
                return;
            }

            if (recCount > 0)
            {
                // update
                insertResult = db.execute_query(@"
update AUTH_WF_EMP_DOC_MAP 
set
    AWEDM_WF_DOC_ID = '" + customWorkflowId + @"',
    AWEDM_UPDATED_USER= '" + user.User_Id + @"', 
    AWEDM_UPDATED_DATE = sysdate
where 
    AWEDM_ESS_DOC_ID ='" + essDocId + @"'
    and  AWEDM_EMP_CODE ='" + empCode + @"'
  
");
                if (insertResult > 0)
                {
                    WriteSuccessMessage("Workflow updated successfully."); return;
                }
            }
            else
            {
                //insert
                insertResult = db.execute_query(@"
INSERT INTO AUTH_WF_EMP_DOC_MAP (
   AWEDM_ESS_DOC_ID, AWEDM_WF_DOC_ID, AWEDM_EMP_CODE, 
   AWEDM_CREATED_DATE, AWEDM_CREATED_USER) 
VALUES (
'" + essDocId + @"',
 '" + customWorkflowId + @"',
 '" + empCode + @"',
 Sysdate,
 '" + user.User_Id + @"')
"); 
                if (insertResult > 0)
                {
                    WriteSuccessMessage("Data inserted successfully."); return;
                }

            }
          
                WriteErrorMessage("Unable to save " + db.error_msg.Replace("'","").Replace("\"",""));


        }
        void ESS_WF_EMP_MAP()
        {

            var query = @"
 

SELECT 
    AWEDM_ESS_DOC_ID, AWEDM_WF_DOC_ID, AWEDM_EMP_CODE
FROM AUTH_WF_EMP_DOC_MAP
    WHERE
        AWEDM_ESS_DOC_ID='" + Request["EssId"] + @"'
     AND AWEDM_EMP_CODE = '" + Request["EmpCode"] + @"'
";

            //AppCode.LogError.WriteToLogFile("", false, query);
            var dt1 = db.execute_query_retun_datatable(query);

            var JSONresult = JsonConvert.SerializeObject(dt1);
            Response.Write(JSONresult);
        }
        void GetEmployeeDetailSAll()
        {

            var SelBranch = Request["SelBranch"];


            if (string.IsNullOrEmpty(SelBranch))
            {
                SelBranch = "ALL";
            }
            var query = @"

select * from (
 SELECT  TO_CHAR(' ') ""id"",TO_CHAR('All') ""name"" FROM dual 
 UNION ALL
 SELECT
   TO_CHAR(PEMP_EMP_CODE) ""id"" , TO_CHAR(PEMP_EMP_NAME) ""name""  FROM PPM_EMPLOYEE_DETAILS
  
    where 
   
    ( upper(PEMP_EMP_CODE) like '%" + Request["q"].ToUpper() + @"%'
or  upper(PEMP_EMP_NAME) like '%" + Request["q"].ToUpper() + @"%' )   
" + (SelBranch != "ALL" ? " and PEMP_EMP_BRANCH_CODE ='" + SelBranch + @"' " : "") + @"
) where rownum<11
";


            //AppCode.LogError.WriteToLogFile("", false, query); //PEMP_EMP_CODE <>  '" + user.Emp_Code + @"' and 
            var dt1 = db.execute_query_retun_datatable(query);

            var JSONresult = JsonConvert.SerializeObject(dt1);
            Response.Write(JSONresult);
            //
        }
        void GetEmployeeDetailSuggestion()
        {

            //user.BranchCode
            var branchOnlyCondition =
                objDB.execute_scalar(@"SELECT NVL(TAP_PARAVALUE,'N') FROM TAS_ATT_PARAM  where  
TAP_PARANAME ='AUTO_COMPLETE_BRANCH_ONLY' ").Equals("Y");

            var query = @"
select * from (
 SELECT
    PEMP_EMP_CODE ""id"",  PEMP_EMP_NAME ""name""
FROM PPM_EMPLOYEE_DETAILS  
    where 
    PEMP_EMP_CODE <>  '" + user.Emp_Code + @"' and 
    ( upper(PEMP_EMP_CODE) like '%" + Request["q"].ToUpper() + @"%'
or  upper(PEMP_EMP_NAME) like '%" + Request["q"].ToUpper() + @"%' )
    " + (branchOnlyCondition ? " AND PEMP_EMP_BRANCH_CODE = '" + user.BranchCode + @"' " : "") + @"
) where rownum<11
";

            //AppCode.LogError.WriteToLogFile("", false, query);
            var dt1 = db.execute_query_retun_datatable(query);

            var JSONresult = JsonConvert.SerializeObject(dt1);
            Response.Write(JSONresult);
            //
        }


        void GetDoctorName()
        {
            EssLeaveRequestDA1 da1 = new EssLeaveRequestDA1();
            //AppCode.LogError.WriteToLogFile("", false, query);
            var dt1 = da1.GetDoctorName(Request["term"]);

            var JSONresult = JsonConvert.SerializeObject(dt1);
            Response.Write(JSONresult);
        }
        void GET_SUGGESTION_FOR_HOSPITAL()
        {
            EssLeaveRequestDA1 da1 = new EssLeaveRequestDA1();
            //AppCode.LogError.WriteToLogFile("", false, query);
            var dt1 = da1.GetHospitalName(Request["term"]);

            var JSONresult = JsonConvert.SerializeObject(dt1);
            Response.Write(JSONresult);
        }


        void GetEmployeeDetailSuggestionWithoutBranch()
        {


            var query = @"
WITH CTE AS (
 SELECT
    PEMP_EMP_CODE ""id"",  PEMP_EMP_NAME ""name""
FROM PPM_EMPLOYEE_DETAILS  
    where 
    ( upper(PEMP_EMP_CODE) like '%" + Request["q"].ToUpper() + @"%'
or  upper(PEMP_EMP_NAME) like '%" + Request["q"].ToUpper() + @"%' )
   and pemp_emp_active='Y' 
) SELECT * FROM CTE where rownum<11
";

            //AppCode.LogError.WriteToLogFile("", false, query);
            var dt1 = db.execute_query_retun_datatable(query);

            var JSONresult = JsonConvert.SerializeObject(dt1);
            Response.Write(JSONresult);
            //
        }

        private void GetEmployeeDetailsActive()
        {
            //user.BranchCode
            //               var branchOnlyCondition =
            //                    objDB.execute_scalar(@"SELECT NVL(TAP_PARAVALUE,'N') FROM TAS_ATT_PARAM  where  
            //TAP_PARANAME ='AUTO_COMPLETE_BRANCH_ONLY' ").Equals("Y");

            var query = @"
select * from (
 SELECT distinct
    PEMP_EMP_CODE ""id"",  PEMP_EMP_NAME ""name""
FROM PPM_EMPLOYEE_DETAILS ,Ess_Leave_Req  
    where PEMP_EMP_CODE=ELR_EMP_ID(+) and ( Elr_Service_Type not in(12,30) ) and
    PEMP_EMP_CODE <>  '" + user.Emp_Code + @"' and 
    ( upper(PEMP_EMP_CODE) like '%" + Request["q"].ToUpper() + @"%'
or  upper(PEMP_EMP_NAME) like '%" + Request["q"].ToUpper() + @"%' )   
) where rownum<11
";

            var dt1 = db.execute_query_retun_datatable(query);

            var JSONresult = JsonConvert.SerializeObject(dt1);
            Response.Write(JSONresult);
            //

        }
        private void GetEmployeeDetails(string Empcode)
        {
            DBAccess db = new DBAccess();

            var strQuery = @"SELECT PEMP_EMP_CODE,PEMP_EMP_NAME,PBM_BRANCH_NAME,PDPM_DEPARTMENT_DESC,
            PEMP_EMP_DESIGNATION_DESC,to_char(PEMP_EMP_JOIN_DATE,'dd/mm/yyyy') as PEMP_EMP_JOIN_DATE,
            to_char(PROB_DATE,'dd/mm/yyyy') PROB_DATE,to_char(sysdate,'dd/mm/yyyy') curdate FROM V_EMPLOYEE_DETAILS
             WHERE PEMP_EMP_CODE='" + Empcode + @"'";


            var dt1 = db.execute_query_retun_datatable(strQuery);

            var JSONresult = JsonConvert.SerializeObject(dt1);
            Response.Write(JSONresult);

        }




        void LOAD_MY_ANNUALHISTORY(string empCode)
        {
            DBAccess ObjDb = new DBAccess();

            var fromDate1 = Request["FromDate"].ToArabicDate();
            var toDate1 = Request["ToDate"].ToArabicDate();
            
            DataTable dt1 = objDB.execute_query_retun_datatable(@"
 


SELECT distinct ELR_LEAVE_REQ_HEAD_ID,
    ELR_EMP_ID, ELR_LEAVE_ID,
TO_CHAR(ELR_FROM_DT,'DD/MM/YYYY')PALE_START_DATE,TO_CHAR(ELR_TO_DT,'DD/MM/YYYY') PALE_END_DATE,
    TO_CHAR(TO_DATE(ELR_TO_DT)+1,'DD/MM/YYYY')TENTATIVEDATE,    
     to_char(ELR_FROM_DT,'dd/mm/yyyy') ELR_FROM_DT1, 
    to_char(ELR_VEH_RETURN_ON,'dd/mm/yyyy') RESUMEDATE, 
     ELR_TOT_DAYS, ELR_BODY, 
    ELR_REQ_STATUS_ID, 
    EMPID, ENAME,to_char(ELR_REQUESTED_DT,'dd/mm/yyyy')REQ_DATE,
    BRANCH_NAME, DEPT_NAME, DIV_NAME, DESIGNATION_DESC,

CASE WHEN ELR_REQ_STATUS_ID = 11  THEN 
            --'Waiting For ' || Replace(INITCAP(AEG_DESCRIPTION),'Waiting For','') 
            GET_LEAVE_STATUS (ELR_REQ_STATUS_ID)     
        ELSE  
            APPROVED_GROUP_NAME
        END   CURR_STATUS_NAME
,
CASE WHEN ELR_REQ_STATUS_ID = 11  THEN 
            --'Waiting For ' || Replace(INITCAP(AEG_DESCRIPTION),'Waiting For','') 
            GET_LEAVE_STATUS (ELR_REQ_STATUS_ID)     
        ELSE  
            APPROVED_GROUP_NAME
        END   APPROVED_GROUP_NAME,
    APP_CNT,TO_CHAR (ELR_APPROVED_DT, 'DD-MM-YYY hh24:mi') APP_DATE,
            TO_CHAR (ELR_APPROVED_DT, 'hh24:mi')APP_TIME
    
FROM 
    ESS_LEAVE_REQ,PPM_PERMISSION_TYPES,
    vu_emp_det,ESS_REQ_APPROVED_GROUP,
      ( select AWDD_WF_HEADER_ROW_ID WF_HEADER_ID, SUM(CASE WHEN wd2.AWDD_STATUS in ('A','R') THEN 1 ELSE 0 END) AS APP_CNT
  from AUTH_WF_DOC_DETAIL wd2  
                    GROUP BY AWDD_WF_HEADER_ROW_ID)
where
    empid=elr_emp_id  
    and ELR_SERVICE_TYPE=26     
     AND UNIQUE_ID_1 =  TO_CHAR(ELR_LEAVE_REQ_HEAD_ID)
                    AND UNIQUE_ID_2 = TO_CHAR(ELR_LEAVE_REQ_HEAD_ID) 
    and WF_HEADER_ID(+) = AWH_ROW_ID1
    
 AND ELR_EMP_ID = '" + empCode + @"'   

 and ((ELR_FROM_DT BETWEEN to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') OR
        ELR_TO_DT BETWEEN to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy')) OR 
          ( ELR_FROM_DT < to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') < ELR_TO_DT ))
   
  order by ELR_LEAVE_REQ_HEAD_ID desc ");
            if (dt1 != null && dt1.Rows.Count > 0)
            {
                var JSONresult = JsonConvert.SerializeObject(dt1);
                Response.Write(JSONresult);
            }
            else
            {
                Response.Write("{}");
            }
        }

        void LOADANNUALAPPROVEDBYME(string empCode)
        {

            var fromDate1 = Request["FromDate"].ToArabicDate();
            var toDate1 = Request["ToDate"].ToArabicDate();
            DBAccess ObjDb = new DBAccess();
          
            string strquery = @"

SELECT distinct ELR_LEAVE_REQ_HEAD_ID,
    ELR_EMP_ID, ELR_LEAVE_ID, TO_CHAR(ELR_FROM_DT,'DD/MM/YYYY')PALE_START_DATE,TO_CHAR(ELR_TO_DT,'DD/MM/YYYY') PALE_END_DATE,
    TO_CHAR(TO_DATE(ELR_TO_DT)+1,'DD/MM/YYYY')TENTATIVEDATE,    
     to_char(ELR_FROM_DT,'dd/mm/yyyy') ELR_FROM_DT1, 
    to_char(ELR_VEH_RETURN_ON,'dd/mm/yyyy') RESUMEDATE,  ELR_TOT_DAYS, ELR_BODY, 
    ELR_REQ_STATUS_ID, 
    EMPID, ENAME,to_char(ELR_REQUESTED_DT,'dd/mm/yyyy')REQ_DATE, 
    BRANCH_NAME, DEPT_NAME, DIV_NAME, DESIGNATION_DESC,APPROVED_GROUP_NAME,
TO_CHAR (ELR_APPROVED_DT, 'DD-MM-YYY hh24:mi') APP_DATE,
            TO_CHAR (ELR_APPROVED_DT, 'hh24:mi')APP_TIME
    
FROM 
    ESS_LEAVE_REQ,
    vu_emp_det,ESS_REQ_APPROVED_GROUP
where
    empid=elr_emp_id  
     and ELR_SERVICE_TYPE= 26     
     AND UNIQUE_ID_1 =  TO_CHAR(ELR_LEAVE_REQ_HEAD_ID)
                    AND UNIQUE_ID_2 = TO_CHAR(ELR_LEAVE_REQ_HEAD_ID) 
    
    and exists ( 
    SELECT 1 FROM AUTH_WF_HEADER, AUTH_WF_DOC_DETAIL
              WHERE     
                    AWDD_WF_HEADER_ROW_ID = AWH_ROW_ID
                    AND AWDD_EMP_APPROVED_BY = '" + empCode + @"'
                    AND AWH_UNIQUE_ID1 =  TO_CHAR(ELR_LEAVE_REQ_HEAD_ID)
                    AND AWH_UNIQUE_ID2 = TO_CHAR(ELR_LEAVE_REQ_HEAD_ID)
    )
and ((ELR_FROM_DT BETWEEN to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') OR
        ELR_TO_DT BETWEEN to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy')) OR 
          ( ELR_FROM_DT < to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') < ELR_TO_DT ))
     order by ELR_LEAVE_REQ_HEAD_ID desc  ";
            //AppCode.LogError.WriteToLogFile("", false, strquery);
            DataTable dt1 = objDB.execute_query_retun_datatable(strquery);
            if (dt1 != null && dt1.Rows.Count > 0)
            {
                var JSONresult = JsonConvert.SerializeObject(dt1);
                Response.Write(JSONresult);
            }
            else
            {
                Response.Write("{}");
            }
        }
        void LOAD_MY_PERMISSIONHISTORY(string empCode)
        {
            DBAccess ObjDb = new DBAccess();

            var fromDate1 = Request["FromDate"].ToArabicDate();
            var toDate1 = Request["ToDate"].ToArabicDate();
            DataTable dt1 = objDB.execute_query_retun_datatable(@"
 


SELECT ELR_LEAVE_REQ_HEAD_ID,
    ELR_EMP_ID, ELR_LEAVE_ID, to_char(ELR_FROM_DT,'dd/mm/yyyy') ELR_FROM_DT1, 
    to_char(ELR_TO_DT,'dd/mm/yyyy') ELR_TO_DT1, ELR_TOT_DAYS, ELR_BODY, 
    ELR_REQ_STATUS_ID, TO_CHAR(TO_DATE(ELR_TRAIN_PURPOSE,'DD/MM/YYYY HH24:MI'),'HH24:MI') ELR_TRAIN_PURPOSE,TO_CHAR(TO_DATE(ELR_TRAIN_GOAL,'DD/MM/YYYY HH24:MI'),'HH24:MI') ELR_TRAIN_GOAL,
    EMPID, ENAME,PPT_PERMISSION_DESC,to_char(ELR_REQUESTED_DT,'dd/mm/yyyy')REQ_DATE,
    BRANCH_NAME, DEPT_NAME, DIV_NAME, DESIGNATION_DESC,

CASE WHEN ELR_REQ_STATUS_ID = 11  THEN 
            --'Waiting For ' || Replace(INITCAP(AEG_DESCRIPTION),'Waiting For','') 
            GET_LEAVE_STATUS (ELR_REQ_STATUS_ID)     
        ELSE  
            APPROVED_GROUP_NAME
        END   CURR_STATUS_NAME
,
CASE WHEN ELR_REQ_STATUS_ID = 11  THEN 
            --'Waiting For ' || Replace(INITCAP(AEG_DESCRIPTION),'Waiting For','') 
            GET_LEAVE_STATUS (ELR_REQ_STATUS_ID)     
        ELSE  
            APPROVED_GROUP_NAME
        END   APPROVED_GROUP_NAME
,
    APP_CNT,TO_CHAR (ELR_APPROVED_DT, 'DD-MM-YYY hh24:mi') APP_DATE,
            TO_CHAR (ELR_APPROVED_DT, 'hh24:mi')APP_TIME
    
FROM 
    ESS_LEAVE_REQ,PPM_PERMISSION_TYPES,
    vu_emp_det,ESS_REQ_APPROVED_GROUP,
      ( select AWDD_WF_HEADER_ROW_ID WF_HEADER_ID, SUM(CASE WHEN wd2.AWDD_STATUS in ('A','R') THEN 1 ELSE 0 END) AS APP_CNT
  from AUTH_WF_DOC_DETAIL wd2  
                    GROUP BY AWDD_WF_HEADER_ROW_ID)
where
    empid=elr_emp_id
    and ELR_SERVICE_TYPE=3
     AND ELR_LEAVE_ID = PPT_PERMISSION_CODE
     AND UNIQUE_ID_1 =  TO_CHAR(ELR_LEAVE_REQ_HEAD_ID)
                    AND UNIQUE_ID_2 = TO_CHAR(ELR_LEAVE_REQ_HEAD_ID) 
    and WF_HEADER_ID(+) = AWH_ROW_ID1
    
 AND ELR_EMP_ID = '" + empCode + @"'   

 and ((ELR_FROM_DT BETWEEN to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') OR
        ELR_TO_DT BETWEEN to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy')) OR 
          ( ELR_FROM_DT < to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') < ELR_TO_DT ))
   
ORDER BY ELR_LEAVE_REQ_HEAD_ID DESC");
            if (dt1 != null && dt1.Rows.Count > 0)
            {
                var fname = "";
                dt1.Columns.Add("FileName");
                foreach (DataRow dr in dt1.Rows)
                {
                    if (dr["ELR_LEAVE_REQ_HEAD_ID"] != "")
                    {
                        fname = Getfilinfo("3", dr["ELR_EMP_ID"].ToString(), dr["ELR_LEAVE_REQ_HEAD_ID"].ToString());
                        dr["FileName"] = fname;
                    }

                }
                var JSONresult = JsonConvert.SerializeObject(dt1);
                Response.Write(JSONresult);
            }
            else
            {
                Response.Write("{}");
            }
        }

        void LOADPERMISSIONAPPROVEDBYME(string empCode)
        {

            var fromDate1 = Request["FromDate"].ToArabicDate();
            var toDate1 = Request["ToDate"].ToArabicDate();
            DBAccess ObjDb = new DBAccess();
            string strquery = @"

SELECT ELR_LEAVE_REQ_HEAD_ID,
    ELR_EMP_ID, ELR_LEAVE_ID, to_char(ELR_FROM_DT,'dd/mm/yyyy') ELR_FROM_DT1, 
    to_char(ELR_TO_DT,'dd/mm/yyyy') ELR_TO_DT1, ELR_TOT_DAYS, ELR_BODY, 
    ELR_REQ_STATUS_ID, TO_CHAR(TO_DATE(ELR_TRAIN_PURPOSE,'DD/MM/YYYY HH24:MI'),'HH24:MI') ELR_TRAIN_PURPOSE,TO_CHAR(TO_DATE(ELR_TRAIN_GOAL,'DD/MM/YYYY HH24:MI'),'HH24:MI') ELR_TRAIN_GOAL,
    EMPID, ENAME,PPT_PERMISSION_DESC,to_char(ELR_REQUESTED_DT,'dd/mm/yyyy')REQ_DATE, 
    BRANCH_NAME, DEPT_NAME, DIV_NAME, DESIGNATION_DESC,APPROVED_GROUP_NAME,
TO_CHAR (ELR_APPROVED_DT, 'DD-MM-YYY hh24:mi') APP_DATE,
            TO_CHAR (ELR_APPROVED_DT, 'hh24:mi')APP_TIME
    
FROM 
    ESS_LEAVE_REQ,PPM_PERMISSION_TYPES,
    vu_emp_det,ESS_REQ_APPROVED_GROUP
where
    empid=elr_emp_id
     and ELR_SERVICE_TYPE= 3
    AND ELR_LEAVE_ID = PPT_PERMISSION_CODE
     AND UNIQUE_ID_1 =  TO_CHAR(ELR_LEAVE_REQ_HEAD_ID)
                    AND UNIQUE_ID_2 = TO_CHAR(ELR_LEAVE_REQ_HEAD_ID) 
    
    and exists ( 
    SELECT 1 FROM AUTH_WF_HEADER, AUTH_WF_DOC_DETAIL
              WHERE     
                    AWDD_WF_HEADER_ROW_ID = AWH_ROW_ID
                    AND AWDD_EMP_APPROVED_BY = '" + empCode + @"'
                    AND AWH_UNIQUE_ID1 =  TO_CHAR(ELR_LEAVE_REQ_HEAD_ID)
                    AND AWH_UNIQUE_ID2 = TO_CHAR(ELR_LEAVE_REQ_HEAD_ID)
    )
and ((ELR_FROM_DT BETWEEN to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') OR
        ELR_TO_DT BETWEEN to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy')) OR 
          ( ELR_FROM_DT < to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') < ELR_TO_DT ))
    ORDER BY ELR_LEAVE_REQ_ID DESC ";

            //AppCode.LogError.WriteToLogFile("", false, strquery);
            DataTable dt1 = objDB.execute_query_retun_datatable(strquery);
            if (dt1 != null && dt1.Rows.Count > 0)
            {
                var fname = "";
                dt1.Columns.Add("FileName");
                foreach (DataRow dr in dt1.Rows)
                {
                    if (dr["ELR_LEAVE_REQ_HEAD_ID"] != "")
                    {
                        fname = Getfilinfo("3", dr["ELR_EMP_ID"].ToString(), dr["ELR_LEAVE_REQ_HEAD_ID"].ToString());
                        dr["FileName"] = fname;
                    }

                }
                var JSONresult = JsonConvert.SerializeObject(dt1);
                Response.Write(JSONresult);
            }
            else
            {
                Response.Write("{}");
            }
        }


        void LOAD_MY_CHECKINHISTORY(string empCode)
        {
            DBAccess ObjDb = new DBAccess();

            var fromDate1 = Request["FromDate"].ToArabicDate();
            var toDate1 = Request["ToDate"].ToArabicDate();
            DataTable dt1 = objDB.execute_query_retun_datatable(@"
 


SELECT  ROW_NUMBER() OVER(ORDER BY ELR_LEAVE_REQ_HEAD_ID DESC)SNO, ELR_LEAVE_REQ_HEAD_ID,
    ELR_EMP_ID, ELR_LEAVE_ID, to_char(ELR_FROM_DT,'dd/mm/yyyy') ELR_FROM_DT1, TO_CHAR(ELR_FROM_DT,'HH24:MI') ELR_FROM_TIME,CASE   ELR_END_DAY_HALF WHEN 'Y'  THEN 'CHECK IN'ELSE 'CHECK OUT' END CHECKSTATUS, 
    to_char(ELR_TO_DT,'dd/mm/yyyy') ELR_TO_DT1, ELR_TOT_DAYS, ELR_BODY, 
    ELR_REQ_STATUS_ID, TO_CHAR(TO_DATE(ELR_TRAIN_PURPOSE,'DD/MM/YYYY HH24:MI'),'HH24:MI') ELR_TRAIN_PURPOSE,TO_CHAR(TO_DATE(ELR_TRAIN_GOAL,'DD/MM/YYYY HH24:MI'),'HH24:MI') ELR_TRAIN_GOAL,
    EMPID, ENAME,to_char(ELR_REQUESTED_DT,'dd/mm/yyyy')REQ_DATE,
    BRANCH_NAME, DEPT_NAME, DIV_NAME, DESIGNATION_DESC,

CASE WHEN ELR_REQ_STATUS_ID = 11  THEN 
            --'Waiting For ' || Replace(INITCAP(AEG_DESCRIPTION),'Waiting For','') 
            GET_LEAVE_STATUS (ELR_REQ_STATUS_ID)     
        ELSE  
            APPROVED_GROUP_NAME
        END   CURR_STATUS_NAME
,
CASE WHEN ELR_REQ_STATUS_ID = 11  THEN 
            --'Waiting For ' || Replace(INITCAP(AEG_DESCRIPTION),'Waiting For','') 
            GET_LEAVE_STATUS (ELR_REQ_STATUS_ID)     
        ELSE  
            APPROVED_GROUP_NAME
        END   APPROVED_GROUP_NAME
,
    APP_CNT,TO_CHAR (ELR_APPROVED_DT, 'DD-MM-YYY hh24:mi') APP_DATE,
            TO_CHAR (ELR_APPROVED_DT, 'hh24:mi')APP_TIME
    
FROM 
    ESS_LEAVE_REQ,
    vu_emp_det,ESS_REQ_APPROVED_GROUP,
      ( select AWDD_WF_HEADER_ROW_ID WF_HEADER_ID, SUM(CASE WHEN wd2.AWDD_STATUS in ('A','R') THEN 1 ELSE 0 END) AS APP_CNT
  from AUTH_WF_DOC_DETAIL wd2  
                    GROUP BY AWDD_WF_HEADER_ROW_ID)
where
    empid=elr_emp_id
    and ELR_SERVICE_TYPE=32
     
     AND UNIQUE_ID_1 =  TO_CHAR(ELR_LEAVE_REQ_HEAD_ID)
                    AND UNIQUE_ID_2 = TO_CHAR(ELR_LEAVE_REQ_HEAD_ID) 
    and WF_HEADER_ID(+) = AWH_ROW_ID1
    
 AND ELR_EMP_ID = '" + empCode + @"'   

 and ((ELR_FROM_DT BETWEEN to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') OR
        ELR_TO_DT BETWEEN to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy')) OR 
          ( ELR_FROM_DT < to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') < ELR_TO_DT ))
   
ORDER BY ELR_LEAVE_REQ_HEAD_ID DESC ");
            if (dt1 != null && dt1.Rows.Count > 0)
            {
                var fname = "";
                dt1.Columns.Add("FileName");
                foreach (DataRow dr in dt1.Rows)
                {
                    if (dr["ELR_LEAVE_REQ_HEAD_ID"] != "")
                    {
                        fname = Getfilinfo("32", dr["ELR_EMP_ID"].ToString(), dr["ELR_LEAVE_REQ_HEAD_ID"].ToString());
                        dr["FileName"] = fname;
                    }

                }
                var JSONresult = JsonConvert.SerializeObject(dt1);
                Response.Write(JSONresult);
            }
            else
            {
                Response.Write("{}");
            }
        }

        void LOADCHECKINAPPROVEDBYME(string empCode)
        {

            var fromDate1 = Request["FromDate"].ToArabicDate();
            var toDate1 = Request["ToDate"].ToArabicDate();
            DBAccess ObjDb = new DBAccess();
            string strquery = @"

SELECT ROW_NUMBER() OVER(ORDER BY ELR_LEAVE_REQ_HEAD_ID DESC)SNO,ELR_LEAVE_REQ_HEAD_ID,
    ELR_EMP_ID, ELR_LEAVE_ID, to_char(ELR_FROM_DT,'dd/mm/yyyy') ELR_FROM_DT1, TO_CHAR(ELR_FROM_DT,'HH24:MI') ELR_FROM_TIME,CASE   ELR_END_DAY_HALF WHEN 'Y'  THEN 'CHECK IN'ELSE 'CHECK OUT' END CHECKSTATUS, 
    to_char(ELR_TO_DT,'dd/mm/yyyy') ELR_TO_DT1, ELR_TOT_DAYS, ELR_BODY, 
    ELR_REQ_STATUS_ID, TO_CHAR(TO_DATE(ELR_TRAIN_PURPOSE,'DD/MM/YYYY HH24:MI'),'HH24:MI') ELR_TRAIN_PURPOSE,TO_CHAR(TO_DATE(ELR_TRAIN_GOAL,'DD/MM/YYYY HH24:MI'),'HH24:MI') ELR_TRAIN_GOAL,
    EMPID, ENAME,to_char(ELR_REQUESTED_DT,'dd/mm/yyyy')REQ_DATE, 
    BRANCH_NAME, DEPT_NAME, DIV_NAME, DESIGNATION_DESC,APPROVED_GROUP_NAME,
TO_CHAR (ELR_APPROVED_DT, 'DD-MM-YYY hh24:mi') APP_DATE,
            TO_CHAR (ELR_APPROVED_DT, 'hh24:mi')APP_TIME
    
FROM 
    ESS_LEAVE_REQ,
    vu_emp_det,ESS_REQ_APPROVED_GROUP
where
    empid=elr_emp_id
     and ELR_SERVICE_TYPE= 32
    
     AND UNIQUE_ID_1 =  TO_CHAR(ELR_LEAVE_REQ_HEAD_ID)
                    AND UNIQUE_ID_2 = TO_CHAR(ELR_LEAVE_REQ_HEAD_ID) 
    
    and exists ( 
    SELECT 1 FROM AUTH_WF_HEADER, AUTH_WF_DOC_DETAIL
              WHERE     
                    AWDD_WF_HEADER_ROW_ID = AWH_ROW_ID
                    AND AWDD_EMP_APPROVED_BY = '" + empCode + @"'
                    AND AWH_UNIQUE_ID1 =  TO_CHAR(ELR_LEAVE_REQ_HEAD_ID)
                    AND AWH_UNIQUE_ID2 = TO_CHAR(ELR_LEAVE_REQ_HEAD_ID)
    )
and ((ELR_FROM_DT BETWEEN to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') OR
        ELR_TO_DT BETWEEN to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy')) OR 
          ( ELR_FROM_DT < to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') < ELR_TO_DT ))
    ORDER BY ELR_LEAVE_REQ_HEAD_ID  DESC";

            //AppCode.LogError.WriteToLogFile("", false, strquery);
            DataTable dt1 = objDB.execute_query_retun_datatable(strquery);
            if (dt1 != null && dt1.Rows.Count > 0)
            {
                var fname = "";
                dt1.Columns.Add("FileName");
                foreach (DataRow dr in dt1.Rows)
                {
                    if (dr["ELR_LEAVE_REQ_HEAD_ID"] != "")
                    {
                        fname = Getfilinfo("3", dr["ELR_EMP_ID"].ToString(), dr["ELR_LEAVE_REQ_HEAD_ID"].ToString());
                        dr["FileName"] = fname;
                    }

                }
                var JSONresult = JsonConvert.SerializeObject(dt1);
                Response.Write(JSONresult);
            }
            else
            {
                Response.Write("{}");
            }
        }

        void LOAD_MY_GETLEAVEDETAILS(string Reqid)
        {
            string attach = ""; string comment = "";
            DBAccess ObjDb = new DBAccess();
            //AND ELR_REQ_STATUS_ID IN(7, 8)
            var fromDate1 = Request["FromDate"].ToArabicDate();
            var toDate1 = Request["ToDate"].ToArabicDate();
            DataTable dt1 = objDB.execute_query_retun_datatable(@"


SELECT ELR_LEAVE_REQ_HEAD_ID,
    ELR_EMP_ID, ELR_LEAVE_ID, to_char(ELR_FROM_DT,'dd/mm/yyyy') ELR_FROM_DT1, 
    to_char(ELR_TO_DT,'dd/mm/yyyy') ELR_TO_DT1, ELR_TOT_DAYS, ELR_BODY, 
    ELR_REQ_STATUS_ID,ELR_SERVICE_TYPE, 
    EMPID, ENAME,PLTM_LEAVE_TYPE_DESC,to_char(ELR_REQUESTED_DT,'dd/mm/yyyy')REQ_DATE,
    BRANCH_NAME, DEPT_NAME, DIV_NAME, DESIGNATION_DESC,

CASE WHEN ELR_REQ_STATUS_ID = 11  THEN 
            --'Waiting For ' || Replace(INITCAP(AEG_DESCRIPTION),'Waiting For','') 
            GET_LEAVE_STATUS (ELR_REQ_STATUS_ID)     
        ELSE  
            APPROVED_GROUP_NAME
        END   CURR_STATUS_NAME
,
CASE WHEN ELR_REQ_STATUS_ID = 11  THEN 
            --'Waiting For ' || Replace(INITCAP(AEG_DESCRIPTION),'Waiting For','') 
            GET_LEAVE_STATUS (ELR_REQ_STATUS_ID)     
        ELSE  
            APPROVED_GROUP_NAME
        END   APPROVED_GROUP_NAME
,
   0 APP_CNT
    
FROM 
    ESS_LEAVE_REQ,ppm_leave_type_master,
    vu_emp_det,ESS_REQ_APPROVED_GROUP,
      ( select AWDD_WF_HEADER_ROW_ID WF_HEADER_ID, SUM(CASE WHEN wd2.AWDD_STATUS in ('A','R') THEN 1 ELSE 0 END) AS APP_CNT
  from AUTH_WF_DOC_DETAIL wd2  
                    GROUP BY AWDD_WF_HEADER_ROW_ID)
where
    empid=elr_emp_id
    and ELR_SERVICE_TYPE=1 
    AND ELR_LEAVE_ID = PLTM_LEAVE_TYPE_CODE
     AND UNIQUE_ID_1 =  TO_CHAR(ELR_LEAVE_REQ_HEAD_ID)
                    AND UNIQUE_ID_2 = TO_CHAR(ELR_LEAVE_REQ_HEAD_ID) 
    and WF_HEADER_ID(+) = AWH_ROW_ID1
    
 AND ELR_LEAVE_REQ_HEAD_ID = '" + Reqid + @"'   

 
ORDER BY ELR_REQUESTED_DT DESC");
            if (dt1 != null && dt1.Rows.Count > 0)
            {
                attach = PouplateATtch(Reqid, dt1.Rows[0]["ELR_EMP_ID"].ToString(), dt1.Rows[0]["ELR_SERVICE_TYPE"].ToString());
                comment = PopulateApproverComments(Reqid);
                DataColumn dc = new DataColumn("Attach", typeof(string));
                DataColumn dc1 = new DataColumn("Comments", typeof(string));
                dt1.Columns.Add(dc);
                dt1.Columns.Add(dc1);

                for (int i = 0; i < dt1.Rows.Count; i++)
                {
                    dt1.Rows[i]["Attach"] = attach;
                    dt1.Rows[i]["Comments"] = comment;
                }

                var JSONresult = JsonConvert.SerializeObject(dt1);
                Response.Write(JSONresult);
            }
            else
            {
                Response.Write("{}");
            }
        }


        public string PouplateATtch(string reqId, string empid, string servicetype)
        {
            string atthData = "";
            var folder = HttpContext.Current.Request.MapPath("~/ESS/Docs/") + "/" + empid + "/"
                    + "/" + servicetype + "/" + reqId;

            try
            {
                Directory.CreateDirectory(Server.MapPath("~/ESS/Docs/" + empid + "/" + servicetype + "/" + reqId));

            }
            catch { }

            if (Directory.Exists(Server.MapPath("~/ESS/Docs/" + empid + "/" + servicetype + "/" + reqId)))
            {
                var files = Directory.GetFiles(folder);

                if (files == null || files.Count() == 0)
                {
                    return atthData;
                }
                var fileInfo = new FileInfo(files[0]);

                var url1 = ResolveClientUrl("~/ESS/Docs/") + "/" + empid + "/"
                    + "/" + servicetype + "/" + reqId + "/" + fileInfo.Name;


                var atthData1 = @"<div class='atth-view1 '>
                                        <div class='file-upload-header'>
                                            Attachments

                                        </div>
                                        <div class='atth-view '>
                                            <ul> 
<li>
	<div class=''>
		<a class='atth1' target='_blacnk' href='" + url1 + @"'><img onerror=""this.onerror=null;this.src='" + ResolveClientUrl("~/images/download.jpg") + @"';"" class='atth-view-img' src='" + url1 + @"'></a>
	</div>
</li>
</ul>
                                        </div>
                                    </div>
";

                atthData = @"  <div class='row'>
                        <div class='col-md-10'>
    " + atthData1 + @"
</div>
                    </div>
";
            }
            return atthData;

        }
        public string PopulateApproverComments(string reqId)
        {
            string approverComments = "";
            var dt1 = db.execute_query_retun_datatable(@"
   SELECT 
       AWDD_ROW_ID, AWDD_WF_HEADER_ROW_ID, AWDD_STATUS, 
       AWDD_COMMENTS,AWDD_EMP_APPROVED_BY, AWDD_APPROVAL_DATE,AWDD_ORDER_BY,
       pemp_emp_name
   FROM 
       AUTH_WF_DOC_DETAIL, AUTH_WF_HEADER, PPM_eMPLOYEE_DETAILS
   where
       AWH_ROW_ID=AWDD_WF_HEADER_ROW_ID
       and AWH_UNIQUE_ID1 =    '" + reqId + @"'
       and AWH_UNIQUE_ID2 =    '" + reqId + @"'
       and AWDD_STATUS in ('A', 'R')
       AND AWDD_EMP_APPROVED_BY = PEMP_EMP_CODE 
   ORDER BY AWDD_ORDER_BY
   ");

            if (dt1.Rows.Count > 0)
            {

                var dynamicRows = "";
                foreach (DataRow item in dt1.Rows)
                {
                    dynamicRows += @"<tr><td>" + item["PEMP_EMP_NAME"].ToString() + @" </td><td>" + item["AWDD_COMMENTS"].ToString() + @"</td></tr>";
                }
                approverComments = @"
   <div class='row'>
       <div class='col-md-10'>
                 <label>" + ERP.Resources.Ess.ApproverComments + @" </label>
                   <table class=""table table-hover"">
                       <thead>
                         <tr>
                           <th>" + ERP.Resources.Ess.ApproverName + @"</th>
                           <th>" + ERP.Resources.Ess.COMMENTS + @"</th>
                         </tr>
                       </thead>
                       <tbody>
                       " + dynamicRows + @"
                   </tbody>
             </table>
       </div>
   </div>

   ";
            }
            return approverComments;


        }

        void LOAD_MY_LEAVEDETAILS(string empCode)
        {
            DBAccess ObjDb = new DBAccess();
            //AND ELR_REQ_STATUS_ID IN(7, 8)  AND NOT  EXISTS
            //(SELECT * FROM ESS_LEAVE_REQ
            //WHERE ELR_SERVICE_TYPE = 28 AND LR.ELR_LEAVE_REQ_HEAD_ID = RTRIM(LTRIM(ELR_LOAN_REFERENCE)))
            var fromDate1 = Request["FromDate"].ToArabicDate();
            var toDate1 = Request["ToDate"].ToArabicDate();
            DataTable dt1 = objDB.execute_query_retun_datatable(@"
 


SELECT ELR_LEAVE_REQ_HEAD_ID,
    ELR_EMP_ID, ELR_LEAVE_ID, to_char(ELR_FROM_DT,'dd/mm/yyyy') ELR_FROM_DT1, 
    to_char(ELR_TO_DT,'dd/mm/yyyy') ELR_TO_DT1, ELR_TOT_DAYS, ELR_BODY, 
    ELR_REQ_STATUS_ID, 
    EMPID, ENAME,PLTM_LEAVE_TYPE_DESC,to_char(ELR_REQUESTED_DT,'dd/mm/yyyy')REQ_DATE,
    BRANCH_NAME, DEPT_NAME, DIV_NAME, DESIGNATION_DESC,

CASE WHEN ELR_REQ_STATUS_ID = 11  THEN 
            --'Waiting For ' || Replace(INITCAP(AEG_DESCRIPTION),'Waiting For','') 
            GET_LEAVE_STATUS (ELR_REQ_STATUS_ID)     
        ELSE  
            APPROVED_GROUP_NAME
        END   CURR_STATUS_NAME
,
CASE WHEN ELR_REQ_STATUS_ID = 11  THEN 
            --'Waiting For ' || Replace(INITCAP(AEG_DESCRIPTION),'Waiting For','') 
            GET_LEAVE_STATUS (ELR_REQ_STATUS_ID)     
        ELSE  
            APPROVED_GROUP_NAME
        END   APPROVED_GROUP_NAME
,
   0 APP_CNT
    
FROM 
    ESS_LEAVE_REQ LR,ppm_leave_type_master,
    vu_emp_det,ESS_REQ_APPROVED_GROUP,
      ( select AWDD_WF_HEADER_ROW_ID WF_HEADER_ID, SUM(CASE WHEN wd2.AWDD_STATUS in ('A','R') THEN 1 ELSE 0 END) AS APP_CNT
  from AUTH_WF_DOC_DETAIL wd2  
                    GROUP BY AWDD_WF_HEADER_ROW_ID)
where
    empid=elr_emp_id
    and ELR_SERVICE_TYPE=1 AND ELR_REQ_STATUS_ID IN(7, 8)

    AND ELR_LEAVE_ID = PLTM_LEAVE_TYPE_CODE
     AND UNIQUE_ID_1 =  TO_CHAR(ELR_LEAVE_REQ_HEAD_ID)
                    AND UNIQUE_ID_2 = TO_CHAR(ELR_LEAVE_REQ_HEAD_ID) 
    and WF_HEADER_ID(+) = AWH_ROW_ID1
    
 AND ELR_EMP_ID = '" + empCode + @"'   

 and ((ELR_FROM_DT BETWEEN to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') OR
        ELR_TO_DT BETWEEN to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy')) OR 
          ( ELR_FROM_DT < to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') < ELR_TO_DT ))
   
ORDER BY ELR_REQUESTED_DT DESC");
            if (dt1 != null && dt1.Rows.Count > 0)
            {
                var JSONresult = JsonConvert.SerializeObject(dt1);
                Response.Write(JSONresult);
            }
            else
            {
                Response.Write("{}");
            }
        }//LOAD_MY_CANCEL_HISTORY LOAD_MY_CANCEL_APPROVED
        void LOAD_MY_CANCEL_HISTORY(string empCode)
        {
            DBAccess ObjDb = new DBAccess();

            var fromDate1 = Request["FromDate"].ToArabicDate();
            var toDate1 = Request["ToDate"].ToArabicDate();
            DataTable dt1 = objDB.execute_query_retun_datatable(@"
 


SELECT ELR_LEAVE_REQ_HEAD_ID,
    ELR_EMP_ID, ELR_LEAVE_ID, to_char(ELR_FROM_DT,'dd/mm/yyyy') ELR_FROM_DT1, 
    to_char(ELR_TO_DT,'dd/mm/yyyy') ELR_TO_DT1, ELR_TOT_DAYS, ELR_BODY, 
    ELR_REQ_STATUS_ID, 
    EMPID, ENAME,PLTM_LEAVE_TYPE_DESC,to_char(ELR_REQUESTED_DT,'dd/mm/yyyy')REQ_DATE,
    BRANCH_NAME, DEPT_NAME, DIV_NAME, DESIGNATION_DESC,

CASE WHEN ELR_REQ_STATUS_ID = 11  THEN 
            --'Waiting For ' || Replace(INITCAP(AEG_DESCRIPTION),'Waiting For','') 
            GET_LEAVE_STATUS (ELR_REQ_STATUS_ID)     
        ELSE  
            APPROVED_GROUP_NAME
        END   CURR_STATUS_NAME
,
CASE WHEN ELR_REQ_STATUS_ID = 11  THEN 
            --'Waiting For ' || Replace(INITCAP(AEG_DESCRIPTION),'Waiting For','') 
            GET_LEAVE_STATUS (ELR_REQ_STATUS_ID)     
        ELSE  
            APPROVED_GROUP_NAME
        END   APPROVED_GROUP_NAME
,
    APP_CNT,TO_CHAR (ELR_APPROVED_DT, 'DD-MM-YYY hh24:mi') APP_DATE,
            TO_CHAR (ELR_APPROVED_DT, 'hh24:mi')APP_TIME
    
FROM 
    ESS_LEAVE_REQ LR,ppm_leave_type_master,
    vu_emp_det,ESS_REQ_APPROVED_GROUP,
      ( select AWDD_WF_HEADER_ROW_ID WF_HEADER_ID, SUM(CASE WHEN wd2.AWDD_STATUS in ('A','R') THEN 1 ELSE 0 END) AS APP_CNT
  from AUTH_WF_DOC_DETAIL wd2  
                    GROUP BY AWDD_WF_HEADER_ROW_ID)
where
    empid=elr_emp_id
    and ELR_SERVICE_TYPE=28
    AND ELR_LEAVE_ID = PLTM_LEAVE_TYPE_CODE
     AND UNIQUE_ID_1 =  TO_CHAR(ELR_LEAVE_REQ_HEAD_ID)
                    AND UNIQUE_ID_2 = TO_CHAR(ELR_LEAVE_REQ_HEAD_ID) 
    and WF_HEADER_ID(+) = AWH_ROW_ID1
    
 AND ELR_EMP_ID = '" + empCode + @"'   

 and ((ELR_FROM_DT BETWEEN to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') OR
        ELR_TO_DT BETWEEN to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy')) OR 
          ( ELR_FROM_DT < to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') < ELR_TO_DT ))
       AND  NOT EXISTS 
          (SELECT * FROM ESS_LEAVE_REQ  EL WHERE  ELR_SERVICE_TYPE=28 AND ELR_LOAN_REFERENCE=LR.ELR_LEAVE_REQ_HEAD_ID   AND ELR_REQ_STATUS_ID IN(3,5,7,8))
 
ORDER BY ELR_LEAVE_REQ_HEAD_ID DESC");
            if (dt1 != null && dt1.Rows.Count > 0)
            {
                var JSONresult = JsonConvert.SerializeObject(dt1);
                Response.Write(JSONresult);
            }
            else
            {
                Response.Write("{}");
            }
        }

        void LOAD_MY_LEAVECHECKBAL()
        {

            var fromDate1 = Request.QueryString["from"];
            var toDate1 = Request["ToDate"].ToArabicDate();
            DBAccess ObjDb = new DBAccess();
            string strquery = @"SELECT PELD_LEAVE_AVAILED_DAYS+TRUNC(NVL(((GET_BALANCE_DAYS(to_date('" + fromDate1 + @"','dd/MM/yyyy')))*(GET_PERDAY_AL('" + user.Emp_Code + @"'))),0),3) bal FROM PPM_EMPLOYEE_LEAVE_DETAILS  
                 where PELD_LEAVE_TYPE='AL' and PELD_CODE='" + user.Emp_Code + "'";

            var ShowBalanceBasedOnDates = "N";

            if(ShowBalanceBasedOnDates == "N")
            {
                strquery = @"SELECT PELD_LEAVE_AVAILED_DAYS bal FROM PPM_EMPLOYEE_LEAVE_DETAILS  
                 where PELD_LEAVE_TYPE='AL' and PELD_CODE='" + user.Emp_Code + "'";

            }

            AppCode.LogError.WriteToLogFile("", false, strquery);
            DataTable dt1 = objDB.execute_query_retun_datatable(strquery);
            if (dt1 != null && dt1.Rows.Count > 0)
            {
                var JSONresult = JsonConvert.SerializeObject(dt1);
                Response.Write(JSONresult);
            }
            else
            {
                Response.Write("{}");
            }
        }
        void LOAD_MY_CANCEL_HISTORYAPPROVEDBYME(string empCode)
        {

            var fromDate1 = Request["FromDate"].ToArabicDate();
            var toDate1 = Request["ToDate"].ToArabicDate();
            DBAccess ObjDb = new DBAccess();
            string strquery = @"

SELECT ELR_LEAVE_REQ_HEAD_ID,
    ELR_EMP_ID, ELR_LEAVE_ID, to_char(ELR_FROM_DT,'dd/mm/yyyy') ELR_FROM_DT1, 
    to_char(ELR_TO_DT,'dd/mm/yyyy') ELR_TO_DT1, ELR_TOT_DAYS, ELR_BODY, 
    ELR_REQ_STATUS_ID,  ELR_TRAIN_PURPOSE,TO_CHAR(TO_DATE(ELR_TRAIN_GOAL,'DD/MM/YYYY HH24:MI'),'HH24:MI') ELR_TRAIN_GOAL,
    EMPID, ENAME,PLTM_LEAVE_TYPE_DESC,to_char(ELR_REQUESTED_DT,'dd/mm/yyyy')REQ_DATE, 
    BRANCH_NAME, DEPT_NAME, DIV_NAME, DESIGNATION_DESC,APPROVED_GROUP_NAME,
TO_CHAR (ELR_APPROVED_DT, 'DD-MM-YYY hh24:mi') APP_DATE,
            TO_CHAR (ELR_APPROVED_DT, 'hh24:mi')APP_TIME
    
FROM 
    ESS_LEAVE_REQ,ppm_leave_type_master,
    vu_emp_det,ESS_REQ_APPROVED_GROUP
where
    empid=elr_emp_id
     and ELR_SERVICE_TYPE= 28
    AND ELR_LEAVE_ID = PLTM_LEAVE_TYPE_CODE
     AND UNIQUE_ID_1 =  TO_CHAR(ELR_LEAVE_REQ_HEAD_ID)
                    AND UNIQUE_ID_2 = TO_CHAR(ELR_LEAVE_REQ_HEAD_ID) 
    
    and exists ( 
    SELECT 1 FROM AUTH_WF_HEADER, AUTH_WF_DOC_DETAIL
              WHERE     
                    AWDD_WF_HEADER_ROW_ID = AWH_ROW_ID
                    AND AWDD_EMP_APPROVED_BY = '" + empCode + @"'
                    AND AWH_UNIQUE_ID1 =  TO_CHAR(ELR_LEAVE_REQ_HEAD_ID)
                    AND AWH_UNIQUE_ID2 = TO_CHAR(ELR_LEAVE_REQ_HEAD_ID)
    )
and ((ELR_FROM_DT BETWEEN to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') OR
        ELR_TO_DT BETWEEN to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy')) OR 
          ( ELR_FROM_DT < to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') < ELR_TO_DT ))
    ORDER BY ELR_LEAVE_REQ_ID DESC ";

            AppCode.LogError.WriteToLogFile("", false, strquery);
            DataTable dt1 = objDB.execute_query_retun_datatable(strquery);
            if (dt1 != null && dt1.Rows.Count > 0)
            {
                var JSONresult = JsonConvert.SerializeObject(dt1);
                Response.Write(JSONresult);
            }
            else
            {
                Response.Write("{}");
            }
        }

        private void LOAD_MYAPPRAISAL_HISTORY(string empCode)
        {
            DBAccess ObjDb = new DBAccess();

            var lang = "en-US";
            var query = @"

SELECT 
	 " + (lang == "en-US" ? @"
 PEMP_EMP_NAME, EST_SERVICE_DESC, PBM_BRANCH_NAME, 
PEMP_EMP_DESIGNATION_DESC, PDPM_DEPARTMENT_DESC, PGDM_GRADE_DESC, CURR_STATUS_NAME," :
     @"
    nvl(PLTM_LEAVE_ARABIC_DESC, PLTM_LEAVE_TYPE_DESC) as PLTM_LEAVE_TYPE_DESC,
    nvl(PEMP_EMP_NAME_AR1,PEMP_EMP_NAME) as PEMP_EMP_NAME, nvl(EST_SERVICE_DESC_AR,EST_SERVICE_DESC) as EST_SERVICE_DESC, 
    nvl(PBM_BRANCH_NAME_ARABIC1, PBM_BRANCH_NAME) as PBM_BRANCH_NAME, nvl(PDPM_DEPT_AR1,PDPM_DEPARTMENT_DESC) as PDPM_DEPARTMENT_DESC, 
    nvl(PDSM_DESIGNATION_DESC_AR1,PEMP_EMP_DESIGNATION_DESC) as PEMP_EMP_DESIGNATION_DESC, nvl(PGDM_GRADE_DESC_AR1, PGDM_GRADE_DESC) as PGDM_GRADE_DESC, nvl(  ELS_STATUS_NAME_AR, CURR_STATUS_NAME) as CURR_STATUS_NAME, "
     )
+ @"
    HAC_CYCLE_NO,HAC_CYCLE_NAME,
    ELRH_LEAVE_REQ_ID,
    EST_SERVICE_ID,
    PEMP_EMP_CODE,
    TO_CHAR (ELRH_REQUESTED_DT, 'dd-mm-yyyy')
    AS ELRH_REQUESTED_DT,
    TO_CHAR (FROM_DT, 'dd-mm-yyyy') AS FROM_DT,
    TO_CHAR (TO_DT, 'dd-mm-yyyy') AS TO_DT,
    ELRH_TOT_DAYS,
    SUBJ,
    LEAVE_BODY,
    ELRH_REQ_STATUS_ID,
     APPROVED_GROUP_NAME
FROM 
    ESS_LEAVE_REQ_DETAILS_ALL, ESS_REQ_APPROVED_GROUP,HAT_APPRAISAL_CYCLE
where 
   ELRH_SERVICE_TYPE in (35)
    AND PEMP_EMP_CODE ='" + empCode + @"'
    AND ELRH_LEAVE_ID = HAC_CYCLE_NO
    and UNIQUE_ID_1(+)=  to_char(ELRH_LEAVE_REQ_ID)
    and UNIQUE_ID_2 (+)=to_char(ELRH_LEAVE_REQ_ID)


 ";
            DataTable dt = ObjDb.execute_query_retun_datatable(query);

            AppCode.LogError.WriteToLogFile("", false, query);

            var rowInfo = @"<tr><td colspan=4>No data found.</td></tr>";

            var empRow = "";
            if (dt != null && dt.Rows.Count > 0)
            {
                int II = 1;
                rowInfo = @"";

                empRow = @"<div >
                                            <div class='col-md-6 '  style='padding-left:0px;'><span style='font-weight: bold;'>" + ERP.Resources.Ess.EmployeeCode + @"</span>: " + dt.Rows[0]["PEMP_EMP_CODE"].ToString() + @"</div>
                                            <div class='col-md-6'> <span  style='font-weight: bold;' >" + ERP.Resources.Ess.EmployeeName + ": </span> " + dt.Rows[0]["PEMP_EMP_NAME"].ToString() + @"</div>
                                        </div>";
                //PEMP_EMP_CODE, PEMP_EMP_NAME PLTM_LEAVE_TYPE_DESC
                foreach (DataRow item in dt.Rows)
                {
                    string leaveHighLisght = "";
                    string foreColorStyle = "";

                    //PLTM_LEAVE_TYPE_DESC
                    rowInfo += @"<tr " + leaveHighLisght + @"><td " + foreColorStyle + @"> " + II + @" </td>
                                                        <td " + foreColorStyle + @">" + item["HAC_CYCLE_NO"].ToString() + @" </td>
                                                        <td " + foreColorStyle + @">" + item["ELRH_TOT_DAYS"].ToString() + @"</td>
                                                        <td " + foreColorStyle + @">" + item["ELRH_REQUESTED_DT"].ToString() + @"</td> 
                                                        <td " + foreColorStyle + @">" + item["APPROVED_GROUP_NAME"].ToString() + @"</td>

                                                    </tr>
";
                    II++;
                }
            }


            var dataToReturn = @"<div class=""portlet light"">
                                    <div class="""">
                                        " + empRow + @"
                                        
                                    </div>
                                    <div class=""portlet-body"">
                                        <div class=""table-scrollable"">
                                            <table class=""table table-hover table-light"">
                                                <thead>
                                                    <tr>
                                                        <th> # </th>
                                                        <th>Appraisal Type</th>
                                                        <th> Final Score </th>
                                                        <th> " + ERP.Resources.Ess.RequestedDate + @" </th> 
                                                        <th> " + ERP.Resources.Ess.Status + @" </th>
                                                    </tr>
                                                </thead>
                                                <tbody>
                                                   " + rowInfo + @"
                                                </tbody>
                                            </table>
                                        </div>
                                    </div>
                                </div>";

            ResponseWrite(dataToReturn);
        }

        void LOAD_MYAPPRAISAL_APPLIED_BY_ME(string empCode)
        {

            var fromDate1 = Request["FromDate"].ToArabicDate();
            var toDate1 = Request["ToDate"].ToArabicDate();
            DBAccess ObjDb = new DBAccess();
            string strquery = @"

SELECT ELR_LEAVE_REQ_HEAD_ID,
    ELR_EMP_ID, ELR_LEAVE_ID, to_char(ELR_FROM_DT,'dd/mm/yyyy') ELR_FROM_DT1, 
    to_char(ELR_TO_DT,'dd/mm/yyyy') ELR_TO_DT1, ELR_TOT_DAYS, ELR_BODY, 
    ELR_REQ_STATUS_ID, 
    EMPID, ENAME,HAC_CYCLE_NO,HAC_CYCLE_NAME,to_char(ELR_REQUESTED_DT,'dd/mm/yyyy')REQ_DATE, 
    BRANCH_NAME, DEPT_NAME, DIV_NAME, DESIGNATION_DESC,APPROVED_GROUP_NAME
    
FROM 
    ESS_LEAVE_REQ,HAT_APPRAISAL_CYCLE,
    vu_emp_det,ESS_REQ_APPROVED_GROUP
where
    empid=elr_emp_id
    and elr_apply_emp_code ='" + empCode + @"'
     and ELR_SERVICE_TYPE= 35
    AND ELR_LEAVE_ID = HAC_CYCLE_NO
     AND UNIQUE_ID_1 =  TO_CHAR(ELR_LEAVE_REQ_HEAD_ID)
                    AND UNIQUE_ID_2 = TO_CHAR(ELR_LEAVE_REQ_HEAD_ID) 
    
    
and ((ELR_FROM_DT BETWEEN to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') OR
        ELR_TO_DT BETWEEN to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy')) OR 
          ( ELR_FROM_DT < to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') < ELR_TO_DT ))
    ORDER BY ELR_LEAVE_REQ_ID DESC ";

             AppCode.LogError.WriteToLogFile("appliedby me", false, strquery);
            DataTable dt1 = objDB.execute_query_retun_datatable(strquery);
            if (dt1 != null && dt1.Rows.Count > 0)
            {
                var JSONresult = JsonConvert.SerializeObject(dt1);
                Response.Write(JSONresult);
            }
            else
            {
                Response.Write("{}");
            }
        }
        void LOAD_MY_APPRAISAL(string empCode)
        {
            DBAccess ObjDb = new DBAccess();

            var fromDate1 = Request["FromDate"].ToArabicDate();
            var toDate1 = Request["ToDate"].ToArabicDate();

            string strappdata = @"

SELECT ELR_LEAVE_REQ_HEAD_ID,
    ELR_EMP_ID,ENAME, ELR_LEAVE_ID, to_char(ELR_FROM_DT,'dd/mm/yyyy') ELR_FROM_DT1, 
    to_char(ELR_TO_DT,'dd/mm/yyyy') ELR_TO_DT1, ELR_TOT_DAYS, ELR_BODY, 
    ELR_REQ_STATUS_ID, 
    EMPID, ENAME,HAC_CYCLE_NO,HAC_CYCLE_NAME,to_char(ELR_REQUESTED_DT,'dd/mm/yyyy')REQ_DATE,
    BRANCH_NAME, DEPT_NAME, DIV_NAME, DESIGNATION_DESC,

CASE WHEN ELR_REQ_STATUS_ID = 11  THEN 
            --'Waiting For ' || Replace(INITCAP(AEG_DESCRIPTION),'Waiting For','') 
            GET_LEAVE_STATUS (ELR_REQ_STATUS_ID)     
        ELSE  
            APPROVED_GROUP_NAME
        END   CURR_STATUS_NAME
,
CASE WHEN ELR_REQ_STATUS_ID = 11  THEN 
            --'Waiting For ' || Replace(INITCAP(AEG_DESCRIPTION),'Waiting For','') 
            GET_LEAVE_STATUS (ELR_REQ_STATUS_ID)     
        ELSE  
            APPROVED_GROUP_NAME
        END   APPROVED_GROUP_NAME,

    APP_CNT, 
TO_CHAR (ELR_APPROVED_DT, 'DD-MM-YYY HH24:MI') APP_DATE,
            TO_CHAR (ELR_APPROVED_DT, 'hh24:mi')APP_TIME  
    
FROM 
    ESS_LEAVE_REQ,HAT_APPRAISAL_CYCLE, 
    vu_emp_det,ESS_REQ_APPROVED_GROUP,
      ( select AWDD_WF_HEADER_ROW_ID WF_HEADER_ID, SUM(CASE WHEN wd2.AWDD_STATUS in ('A','R') THEN 1 ELSE 0 END) AS APP_CNT
  from AUTH_WF_DOC_DETAIL wd2  
                    GROUP BY AWDD_WF_HEADER_ROW_ID)
where
    EMPID=ELR_EMP_ID  
    and ELR_SERVICE_TYPE=35
    AND ELR_LEAVE_ID = HAC_CYCLE_NO
     AND UNIQUE_ID_1 =  TO_CHAR(ELR_LEAVE_REQ_HEAD_ID)
                    AND UNIQUE_ID_2 = TO_CHAR(ELR_LEAVE_REQ_HEAD_ID) 
    and WF_HEADER_ID(+) = AWH_ROW_ID1
    
 AND ELR_APPLY_EMP_CODE = '" + empCode + @"'   

 and ((ELR_FROM_DT BETWEEN to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') OR
        ELR_TO_DT BETWEEN to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy')) OR 
          ( ELR_FROM_DT < to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') < ELR_TO_DT ))
   
ORDER BY ELR_LEAVE_REQ_HEAD_ID DESC";


            DataTable dt1 = objDB.execute_query_retun_datatable(strappdata);


            AppCode.LogError.WriteToLogFile("historyfor apprisal", false, strappdata);
            if (dt1 != null && dt1.Rows.Count > 0)
            {
                var fname = "";
                dt1.Columns.Add("FileName");
                foreach (DataRow dr in dt1.Rows)
                {
                    if (dr["ELR_LEAVE_REQ_HEAD_ID"] != "")
                    {
                        fname = Getfilinfo("35", dr["ELR_EMP_ID"].ToString(), dr["ELR_LEAVE_REQ_HEAD_ID"].ToString());
                        dr["FileName"] = fname;
                    }

                }

                var JSONresult = JsonConvert.SerializeObject(dt1);
                Response.Write(JSONresult);
            }
            else
            {
                Response.Write("{}");
            }
        }

        void LOADMYAPPRAISALHISTORYAPPROVEDBYME(string empCode)
        {

            var fromDate1 = Request["FromDate"].ToArabicDate();
            var toDate1 = Request["ToDate"].ToArabicDate();
            DBAccess ObjDb = new DBAccess();
            string strquery = @"

SELECT ELR_LEAVE_REQ_HEAD_ID,
    ELR_EMP_ID, ELR_LEAVE_ID, to_char(ELR_FROM_DT,'dd/mm/yyyy') ELR_FROM_DT1, 
    to_char(ELR_TO_DT,'dd/mm/yyyy') ELR_TO_DT1, ELR_TOT_DAYS, ELR_BODY, 
    ELR_REQ_STATUS_ID, 
    EMPID, ENAME,HAC_CYCLE_NO,HAC_CYCLE_NAME,to_char(ELR_REQUESTED_DT,'dd/mm/yyyy')REQ_DATE, 
    BRANCH_NAME, DEPT_NAME, DIV_NAME, DESIGNATION_DESC,APPROVED_GROUP_NAME,
    TO_CHAR (ELR_APPROVED_DT, 'DD-MM-YYY hh24:mi') APP_DATE,
            TO_CHAR (ELR_APPROVED_DT, 'hh24:mi')APP_TIME
    
FROM 
    ESS_LEAVE_REQ,HAT_APPRAISAL_CYCLE,
    vu_emp_det,ESS_REQ_APPROVED_GROUP
where
    empid=elr_emp_id  
     and ELR_SERVICE_TYPE= 35
    AND ELR_LEAVE_ID = HAC_CYCLE_NO
     AND UNIQUE_ID_1 =  TO_CHAR(ELR_LEAVE_REQ_HEAD_ID)
                    AND UNIQUE_ID_2 = TO_CHAR(ELR_LEAVE_REQ_HEAD_ID) 
    
    and exists ( 
    SELECT 1 FROM AUTH_WF_HEADER, AUTH_WF_DOC_DETAIL
              WHERE     
                    AWDD_WF_HEADER_ROW_ID = AWH_ROW_ID
                    AND AWDD_EMP_APPROVED_BY = '" + empCode + @"'
                    AND AWH_UNIQUE_ID1 =  TO_CHAR(ELR_LEAVE_REQ_HEAD_ID)
                    AND AWH_UNIQUE_ID2 = TO_CHAR(ELR_LEAVE_REQ_HEAD_ID)
    )
and ((ELR_FROM_DT BETWEEN to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') OR
        ELR_TO_DT BETWEEN to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy')) OR 
          ( ELR_FROM_DT < to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') < ELR_TO_DT ))
    ORDER BY ELR_LEAVE_REQ_ID DESC ";

            AppCode.LogError.WriteToLogFile("approvedbyme", false, strquery);
            DataTable dt1 = objDB.execute_query_retun_datatable(strquery);
            if (dt1 != null && dt1.Rows.Count > 0)
            {
                var fname = "";
                dt1.Columns.Add("FileName");
                foreach (DataRow dr in dt1.Rows)
                {
                    if (dr["ELR_LEAVE_REQ_HEAD_ID"] != "")
                    {
                        fname = Getfilinfo("35", dr["ELR_EMP_ID"].ToString(), dr["ELR_LEAVE_REQ_HEAD_ID"].ToString());
                        dr["FileName"] = fname;
                    }

                }
                var JSONresult = JsonConvert.SerializeObject(dt1);
                Response.Write(JSONresult);
            }
            else
            {
                Response.Write("{}");
            }
        }

        void LOAD_MY_LEAVEPLANNER_APPROVEDBYME(string empCode)
        {

            var fromDate1 = Request["FromDate"].ToArabicDate();
            var toDate1 = Request["ToDate"].ToArabicDate();
            DBAccess ObjDb = new DBAccess();
            string strquery = @"

SELECT ELR_LEAVE_REQ_HEAD_ID,
    ELR_EMP_ID, ELR_LEAVE_ID, to_char(ELR_FROM_DT,'dd/mm/yyyy') ELR_FROM_DT1, 
    to_char(ELR_TO_DT,'dd/mm/yyyy') ELR_TO_DT1, ELR_TOT_DAYS, ELR_BODY, 
    ELR_REQ_STATUS_ID, 
    EMPID, ENAME,PLTM_LEAVE_TYPE_DESC,to_char(ELR_REQUESTED_DT,'dd/mm/yyyy')REQ_DATE, 
    BRANCH_NAME, DEPT_NAME, DIV_NAME, DESIGNATION_DESC,APPROVED_GROUP_NAME,
    TO_CHAR (ELR_APPROVED_DT, 'DD-MM-YYY hh24:mi') APP_DATE,
            TO_CHAR (ELR_APPROVED_DT, 'hh24:mi')APP_TIME
    
FROM 
    ESS_LEAVE_REQ,ppm_leave_type_master,
    vu_emp_det,ESS_REQ_APPROVED_GROUP
where
    empid=elr_emp_id
     and ELR_SERVICE_TYPE= 37
    AND ELR_LEAVE_ID = PLTM_LEAVE_TYPE_CODE
     AND UNIQUE_ID_1 =  TO_CHAR(ELR_LEAVE_REQ_HEAD_ID)
                    AND UNIQUE_ID_2 = TO_CHAR(ELR_LEAVE_REQ_HEAD_ID) 
    
    and exists ( 
    SELECT 1 FROM AUTH_WF_HEADER, AUTH_WF_DOC_DETAIL
              WHERE     
                    AWDD_WF_HEADER_ROW_ID = AWH_ROW_ID
                    AND AWDD_EMP_APPROVED_BY = '" + empCode + @"'
                    AND AWH_UNIQUE_ID1 =  TO_CHAR(ELR_LEAVE_REQ_HEAD_ID)
                    AND AWH_UNIQUE_ID2 = TO_CHAR(ELR_LEAVE_REQ_HEAD_ID)
    )
and ((ELR_FROM_DT BETWEEN to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') OR
        ELR_TO_DT BETWEEN to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy')) OR 
          ( ELR_FROM_DT < to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') < ELR_TO_DT ))
    ORDER BY ELR_LEAVE_REQ_ID DESC ";

            //AppCode.LogError.WriteToLogFile("", false, strquery);
            DataTable dt1 = objDB.execute_query_retun_datatable(strquery);
            if (dt1 != null && dt1.Rows.Count > 0)
            {
                var fname = "";
                dt1.Columns.Add("FileName");
                foreach (DataRow dr in dt1.Rows)
                {
                    if (dr["ELR_LEAVE_REQ_HEAD_ID"] != "")
                    {
                        fname = Getfilinfo("37", dr["ELR_EMP_ID"].ToString(), dr["ELR_LEAVE_REQ_HEAD_ID"].ToString());
                        dr["FileName"] = fname;
                    }

                }
                var JSONresult = JsonConvert.SerializeObject(dt1);
                Response.Write(JSONresult);
            }
            else
            {
                Response.Write("{}");
            }
        }
        void LOAD_MY_LEAVEPLANNER(string empCode)
        {
            DBAccess ObjDb = new DBAccess();

            var fromDate1 = Request["FromDate"].ToArabicDate();
            var toDate1 = Request["ToDate"].ToArabicDate();
            DataTable dt1 = objDB.execute_query_retun_datatable(@"
 


SELECT ELR_LEAVE_REQ_HEAD_ID,
    ELR_EMP_ID, ELR_LEAVE_ID, to_char(ELR_FROM_DT,'dd/mm/yyyy') ELR_FROM_DT1, 
    to_char(ELR_TO_DT,'dd/mm/yyyy') ELR_TO_DT1, ELR_TOT_DAYS, ELR_BODY, 
    ELR_REQ_STATUS_ID, 
    EMPID, ENAME,PLTM_LEAVE_TYPE_DESC,to_char(ELR_REQUESTED_DT,'dd/mm/yyyy')REQ_DATE,
    BRANCH_NAME, DEPT_NAME, DIV_NAME, DESIGNATION_DESC,

CASE WHEN ELR_REQ_STATUS_ID = 11  THEN 
            --'Waiting For ' || Replace(INITCAP(AEG_DESCRIPTION),'Waiting For','') 
            GET_LEAVE_STATUS (ELR_REQ_STATUS_ID)     
        ELSE  
            APPROVED_GROUP_NAME
        END   CURR_STATUS_NAME
,
CASE WHEN ELR_REQ_STATUS_ID = 11  THEN 
            --'Waiting For ' || Replace(INITCAP(AEG_DESCRIPTION),'Waiting For','') 
            GET_LEAVE_STATUS (ELR_REQ_STATUS_ID)     
        ELSE  
            APPROVED_GROUP_NAME
        END   APPROVED_GROUP_NAME
,
    APP_CNT, 
TO_CHAR (ELR_APPROVED_DT, 'DD-MM-YYY HH24:MI') APP_DATE,
            TO_CHAR (ELR_APPROVED_DT, 'hh24:mi')APP_TIME  
    
FROM 
    ESS_LEAVE_REQ,ppm_leave_type_master,
    vu_emp_det,ESS_REQ_APPROVED_GROUP,
      ( select AWDD_WF_HEADER_ROW_ID WF_HEADER_ID, SUM(CASE WHEN wd2.AWDD_STATUS in ('A','R') THEN 1 ELSE 0 END) AS APP_CNT
  from AUTH_WF_DOC_DETAIL wd2  
                    GROUP BY AWDD_WF_HEADER_ROW_ID)
where
    empid=elr_emp_id
    and ELR_SERVICE_TYPE=37
    AND ELR_LEAVE_ID = PLTM_LEAVE_TYPE_CODE
     AND UNIQUE_ID_1 =  TO_CHAR(ELR_LEAVE_REQ_HEAD_ID)
                    AND UNIQUE_ID_2 = TO_CHAR(ELR_LEAVE_REQ_HEAD_ID) 
    and WF_HEADER_ID(+) = AWH_ROW_ID1
    
 AND ELR_EMP_ID = '" + empCode + @"'   

 and ((ELR_FROM_DT BETWEEN to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') OR
        ELR_TO_DT BETWEEN to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy')) OR 
          ( ELR_FROM_DT < to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') < ELR_TO_DT ))
   
ORDER BY ELR_LEAVE_REQ_HEAD_ID DESC");
            if (dt1 != null && dt1.Rows.Count > 0)
            {
                var fname = "";
                dt1.Columns.Add("FileName");
                foreach (DataRow dr in dt1.Rows)
                {
                    if (dr["ELR_LEAVE_REQ_HEAD_ID"] != "")
                    {
                        fname = Getfilinfo("37", dr["ELR_EMP_ID"].ToString(), dr["ELR_LEAVE_REQ_HEAD_ID"].ToString());
                        dr["FileName"] = fname;
                    }

                }

                var JSONresult = JsonConvert.SerializeObject(dt1);
                Response.Write(JSONresult);
            }
            else
            {
                Response.Write("{}");
            }
        }
        void LOAD_MY_LEAVES(string empCode)
        {
            DBAccess ObjDb = new DBAccess();

            var fromDate1 = Request["FromDate"].ToArabicDate();
            var toDate1 = Request["ToDate"].ToArabicDate();
            string par_command = @"
 


SELECT ELR_LEAVE_REQ_HEAD_ID,
    ELR_EMP_ID, ELR_LEAVE_ID, to_char(ELR_FROM_DT,'dd/mm/yyyy') ELR_FROM_DT1, 
    to_char(ELR_TO_DT,'dd/mm/yyyy') ELR_TO_DT1, ELR_TOT_DAYS, ELR_BODY, 
    ELR_REQ_STATUS_ID, 
    EMPID, ENAME,PLTM_LEAVE_TYPE_DESC,to_char(ELR_REQUESTED_DT,'dd/mm/yyyy')REQ_DATE,
    BRANCH_NAME, DEPT_NAME, DIV_NAME, DESIGNATION_DESC,

CASE WHEN ELR_REQ_STATUS_ID = 11  THEN 
            --'Waiting For ' || Replace(INITCAP(AEG_DESCRIPTION),'Waiting For','') 
            GET_LEAVE_STATUS (ELR_REQ_STATUS_ID)     
        ELSE  
            APPROVED_GROUP_NAME
        END   CURR_STATUS_NAME
,
CASE WHEN ELR_REQ_STATUS_ID = 11  THEN 
            --'Waiting For ' || Replace(INITCAP(AEG_DESCRIPTION),'Waiting For','') 
            GET_LEAVE_STATUS (ELR_REQ_STATUS_ID)     
        ELSE  
            APPROVED_GROUP_NAME
        END   APPROVED_GROUP_NAME
,
    APP_CNT, 
TO_CHAR (ELR_APPROVED_DT, 'DD-MM-YYY HH24:MI') APP_DATE,
            TO_CHAR (ELR_APPROVED_DT, 'hh24:mi')APP_TIME  
    
FROM 
    ESS_LEAVE_REQ,ppm_leave_type_master,
    vu_emp_det,ESS_REQ_APPROVED_GROUP,
      ( select AWDD_WF_HEADER_ROW_ID WF_HEADER_ID, SUM(CASE WHEN wd2.AWDD_STATUS in ('A','R') THEN 1 ELSE 0 END) AS APP_CNT
  from AUTH_WF_DOC_DETAIL wd2  
                    GROUP BY AWDD_WF_HEADER_ROW_ID)
where
    empid=elr_emp_id
    and ELR_SERVICE_TYPE=1
    AND ELR_LEAVE_ID = PLTM_LEAVE_TYPE_CODE
     AND UNIQUE_ID_1 =  TO_CHAR(ELR_LEAVE_REQ_HEAD_ID)
                    AND UNIQUE_ID_2 = TO_CHAR(ELR_LEAVE_REQ_HEAD_ID) 
    and WF_HEADER_ID(+) = AWH_ROW_ID1
    
 AND ELR_EMP_ID = '" + empCode + @"'   

 and ((ELR_FROM_DT BETWEEN to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') OR
        ELR_TO_DT BETWEEN to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy')) OR 
          ( ELR_FROM_DT < to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') < ELR_TO_DT ))
   
ORDER BY ELR_LEAVE_REQ_HEAD_ID DESC";
            AppCode.LogError.WriteToLogFile("", false, par_command);
            DataTable dt1 = objDB.execute_query_retun_datatable(par_command);
            if (dt1 != null && dt1.Rows.Count > 0)
            {
                var fname = "";
                dt1.Columns.Add("FileName");
                foreach (DataRow dr in dt1.Rows)
                {
                    if (dr["ELR_LEAVE_REQ_HEAD_ID"] != "")
                    {
                        fname = Getfilinfo("1", dr["ELR_EMP_ID"].ToString(), dr["ELR_LEAVE_REQ_HEAD_ID"].ToString());
                        dr["FileName"] = fname;
                    }

                }

                var JSONresult = JsonConvert.SerializeObject(dt1);
                Response.Write(JSONresult);
            }
            else
            {
                Response.Write("{}");
            }
        }

        void LOAD_MY_APPOINTMENT_HISTORY(string empCode)
        {
            DBAccess ObjDb = new DBAccess();

            var fromDate1 = Request["FromDate"].ToArabicDate();
            var toDate1 = Request["ToDate"].ToArabicDate();

            var strquery = @"SELECT ELR_LEAVE_REQ_HEAD_ID,VAD_APPOINTMENT_NO,VAD_VISITOR_NAME, VAD_VISITOR_COMPANY, to_char(VAD_APPOINTMENT_START_DATE,'dd/mm/yyyy')VAD_APPOINTMENT_START_DATE,
    to_char(VAD_APPOINTMENT_END_DATE,'dd/mm/yyyy')  VAD_APPOINTMENT_END_DATE,  VAD_VISITOR_COMPANY,ELR_EMP_ID,  to_char(ELR_FROM_DT,'dd/mm/yyyy') ELR_FROM_DT1, 
    to_char(ELR_TO_DT,'dd/mm/yyyy') ELR_TO_DT1, ELR_TOT_DAYS, ELR_BODY, 
    ELR_REQ_STATUS_ID, REPLACE(UPPER(VMS_VIST_CAT.MCMD_ENTITY_DESC),'GATE PASS','') CATEGORY_DESC,
    (EMPID||'-'|| ENAME)ENAME,to_char(ELR_REQUESTED_DT,'dd/mm/yyyy')REQ_DATE,
    BRANCH_NAME, DEPT_NAME, DIV_NAME, DESIGNATION_DESC,

CASE WHEN ELR_REQ_STATUS_ID = 11  THEN 
            --'Waiting For ' || Replace(INITCAP(AEG_DESCRIPTION),'Waiting For','') 
            GET_LEAVE_STATUS (ELR_REQ_STATUS_ID)    
       WHEN ELR_REQ_STATUS_ID = 1  THEN             
            GET_LEAVE_STATUS (ELR_REQ_STATUS_ID) 
        WHEN ELR_REQ_STATUS_ID = 2  THEN              
            GET_LEAVE_STATUS (ELR_REQ_STATUS_ID)         
        ELSE  
            APPROVED_GROUP_NAME
        END   APPROVED_GROUP_NAME, 
    APP_CNT, 
TO_CHAR (ELR_APPROVED_DT, 'DD-MM-YYY HH24:MI') APP_DATE,
            TO_CHAR (ELR_APPROVED_DT, 'hh24:mi')APP_TIME  
    
FROM 
    ESS_LEAVE_REQ,VMT_APPOINTMENT_DETAILS,
    vu_emp_det,ESS_REQ_APPROVED_GROUP,MMM_COMMON_MASTERS_DETAIL VMS_VIST_CAT,
      ( select AWDD_WF_HEADER_ROW_ID WF_HEADER_ID, SUM(CASE WHEN wd2.AWDD_STATUS in ('A','R') THEN 1 ELSE 0 END) AS APP_CNT
        from AUTH_WF_DOC_DETAIL wd2  
         GROUP BY AWDD_WF_HEADER_ROW_ID)
where
    empid=elr_emp_id
    AND ELR_SERVICE_TYPE=38
    AND TO_CHAR (VAD_APPOINTMENT_ID) = ELR_REF_DOC_NO
     AND UNIQUE_ID_1(+) =  TO_CHAR(ELR_LEAVE_REQ_HEAD_ID)
     AND UNIQUE_ID_2(+) = TO_CHAR(ELR_LEAVE_REQ_HEAD_ID) 
    AND WF_HEADER_ID(+) = AWH_ROW_ID1
    AND VMS_VIST_CAT.MCMD_ENTITY_GROUP(+)='VMS_VIST_CAT' AND   VAD_VISITOR_CATEGORY=VMS_VIST_CAT.MCMD_ENTITY_CODE(+)
 AND ELR_EMP_ID = '" + empCode + @"'   

 AND ((ELR_FROM_DT BETWEEN to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') OR
        ELR_TO_DT BETWEEN to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy')) OR 
          ( ELR_FROM_DT < to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') < ELR_TO_DT ))
   
ORDER BY ELR_LEAVE_REQ_HEAD_ID DESC";
            log_error.write_to_log_file("appointment History", "true", strquery);
  DataTable dt1 = objDB.execute_query_retun_datatable(strquery);

            if (dt1 != null && dt1.Rows.Count > 0)
            {
                var fname = "";
                dt1.Columns.Add("FileName");
                foreach (DataRow dr in dt1.Rows)
                {
                    if (dr["ELR_LEAVE_REQ_HEAD_ID"] != "")
                    {
                        fname = Getfilinfo("1", dr["ELR_EMP_ID"].ToString(), dr["ELR_LEAVE_REQ_HEAD_ID"].ToString());
                        dr["FileName"] = fname;
                    }

                }

                var JSONresult = JsonConvert.SerializeObject(dt1);
                Response.Write(JSONresult);
            }
            else
            {
                Response.Write("{}");
            }
        }


        void LOAD_MY_APPOINTMENT_APPROVED_VIEW(string empCode)
        {
            DBAccess ObjDb = new DBAccess();

            var fromDate1 = Request["FromDate"].ToArabicDate();
            var toDate1 = Request["ToDate"].ToArabicDate();




            var strquery = @"
SELECT distinct ELR_LEAVE_REQ_HEAD_ID,VAD_APPOINTMENT_NO,VAD_VISITOR_NAME, VAD_VISITOR_COMPANY, to_char(VAD_APPOINTMENT_START_DATE,'dd/mm/yyyy')VAD_APPOINTMENT_START_DATE,
    to_char(VAD_APPOINTMENT_END_DATE,'dd/mm/yyyy')  VAD_APPOINTMENT_END_DATE,  VAD_VISITOR_COMPANY,ELR_EMP_ID,  to_char(ELR_FROM_DT,'dd/mm/yyyy') ELR_FROM_DT1, 
    to_char(ELR_TO_DT,'dd/mm/yyyy') ELR_TO_DT1, ELR_TOT_DAYS, ELR_BODY, VAD_VISITOR_ID_NUMBER,
    ELR_REQ_STATUS_ID, REPLACE(UPPER(VMS_VIST_CAT.MCMD_ENTITY_DESC),'GATE PASS','') CATEGORY_DESC,
    (EMPID||'-'|| ENAME)ENAME,to_char(ELR_REQUESTED_DT,'dd/mm/yyyy')REQ_DATE,
    BRANCH_NAME, DEPT_NAME, DIV_NAME, DESIGNATION_DESC, LOCATION_NAME || '(' || LOCATION_TAG || ')' AS LOCATION_TAG,
to_char(VAD_APPOINTMENT_START_DATE,'dd/mm/yyyy')||'/'||to_char(VAD_APPOINTMENT_END_DATE,'dd/mm/yyyy')   START_END_DATE,
 to_char( FROM_TZ( CAST( VAD_APPOINTMENT_START_time AS TIMESTAMP ), 'UTC' )AT TIME ZONE 'Asia/Muscat' ,'HH:MI') || '/'||
to_char( FROM_TZ( CAST( VAD_APPOINTMENT_end_time AS TIMESTAMP ), 'UTC' )AT TIME ZONE 'Asia/Muscat' ,'HH:MI')  START_END_TIME,
CASE WHEN ELR_REQ_STATUS_ID = 11  THEN 
            --'Waiting For ' || Replace(INITCAP(AEG_DESCRIPTION),'Waiting For','') 
            GET_LEAVE_STATUS (ELR_REQ_STATUS_ID)    
       WHEN ELR_REQ_STATUS_ID = 1  THEN             
            GET_LEAVE_STATUS (ELR_REQ_STATUS_ID) 
        WHEN ELR_REQ_STATUS_ID = 2  THEN              
            GET_LEAVE_STATUS (ELR_REQ_STATUS_ID)         
        ELSE  
            APPROVED_GROUP_NAME
        END   APPROVED_GROUP_NAME, 
    APP_CNT, 
TO_CHAR (ELR_APPROVED_DT, 'DD-MM-YYY HH24:MI') APP_DATE,
            TO_CHAR (ELR_APPROVED_DT, 'hh24:mi')APP_TIME  
    
FROM 
    ESS_LEAVE_REQ,VMT_APPOINTMENT_DETAILS,C_CAFM_LOCATIONS,
    vu_emp_det,ESS_REQ_APPROVED_GROUP,MMM_COMMON_MASTERS_DETAIL VMS_VIST_CAT,ESS_WF_DETAIL,
      ( select AWDD_WF_HEADER_ROW_ID WF_HEADER_ID, SUM(CASE WHEN wd2.AWDD_STATUS in ('A','R') THEN 1 ELSE 0 END) AS APP_CNT
        from AUTH_WF_DOC_DETAIL wd2  
         GROUP BY AWDD_WF_HEADER_ROW_ID)
where
    empid=elr_emp_id
    AND ELR_SERVICE_TYPE=38
    AND TO_CHAR (VAD_APPOINTMENT_ID) = ELR_REF_DOC_NO
     AND UNIQUE_ID_1(+) =  TO_CHAR(ELR_LEAVE_REQ_HEAD_ID)
     AND UNIQUE_ID_2(+) = TO_CHAR(ELR_LEAVE_REQ_HEAD_ID)  AND AWH_UNIQUE_ID1 =  TO_CHAR(ELR_LEAVE_REQ_HEAD_ID)
    AND WF_HEADER_ID(+) = AWH_ROW_ID1  
    AND VMS_VIST_CAT.MCMD_ENTITY_GROUP(+)='VMS_VIST_CAT' AND   VAD_VISITOR_CATEGORY=VMS_VIST_CAT.MCMD_ENTITY_CODE(+)
        AND ELR_REQ_STATUS_ID IN(7,8) AND GROUP_APRROVED_EMP_CODE='" + empCode + @"'
            AND VAD_LOCATION = LOCATION_ID(+)";

            strquery += " AND((ELR_FROM_DT BETWEEN to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') OR
        ELR_TO_DT BETWEEN to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy')) OR 
          ( ELR_FROM_DT < to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') < ELR_TO_DT ))
   
ORDER BY ELR_LEAVE_REQ_HEAD_ID DESC";

            log_error.write_to_log_file("Appointment Approvedbyme", "true", strquery);
            DataTable dt1 = objDB.execute_query_retun_datatable(strquery);

            if (dt1 != null && dt1.Rows.Count > 0)
            {
                var fname = "";
                dt1.Columns.Add("FileName");
                foreach (DataRow dr in dt1.Rows)
                {
                    if (dr["ELR_LEAVE_REQ_HEAD_ID"] != "")
                    {
                        fname = Getfilinfo("1", dr["ELR_EMP_ID"].ToString(), dr["ELR_LEAVE_REQ_HEAD_ID"].ToString());
                        dr["FileName"] = fname;
                    }

                }

                var JSONresult = JsonConvert.SerializeObject(dt1);
                Response.Write(JSONresult);
            }
            else
            {
                Response.Write("{}");
            }
        }
        public string GetfilinfoHR(string servicetype, string empid, string reqid)
        {

            var Crfolder = HttpContext.Current.Request.MapPath("~/ESS/Docs/") + "/" + empid + "/";


            if (Directory.Exists(Crfolder))
            {
                var folder = "";

                folder = HttpContext.Current.Request.MapPath("~/ESS/Docs/") + "/" + empid + "/"
                  + "/" + servicetype.ToString() + "/" + reqid + "/HrDocs/";

                if (Directory.Exists(folder))
                {
                    var files = Directory.GetFiles(folder);

                    if (files == null || files.Count() == 0)
                    {
                        return "";
                    }
                    var fileInfo = new FileInfo(files[0]);
                    var url1 = "";

                    url1 = ResolveClientUrl("~/ESS/Docs/") + "/" + empid + "/"
                   + "/" + servicetype.ToString() + "/" + reqid + "/HrDocs/" + fileInfo.Name;

                    return url1;
                }
                else
                    return "";
            }
            else
                return "";

        }

        public string Getfilinfo(string servicetype, string empid, string reqid)
        {

            var Crfolder = HttpContext.Current.Request.MapPath("~/ESS/Docs/") + "/" + empid + "/";


            if (Directory.Exists(Crfolder))
            {
                var folder = "";
                if (servicetype == "34")
                    folder = HttpContext.Current.Request.MapPath("~/ESS/Docs/") + "/" + empid + "/"
                     + "/Transfer";
                else
                    folder = HttpContext.Current.Request.MapPath("~/ESS/Docs/") + "/" + empid + "/"
                      + "/" + servicetype.ToString() + "/" + reqid;

                if (Directory.Exists(folder))
                {
                    var files = Directory.GetFiles(folder);

                    if (files == null || files.Count() == 0)
                    {
                        return "";
                    }
                    var fileInfo = new FileInfo(files[0]);
                    var url1 = "";
                    if (servicetype == "34")
                        url1 = ResolveClientUrl("~/ESS/Docs/") + "/" + empid + "/"
                        + "/Transfer/" + fileInfo.Name;
                    else
                        url1 = ResolveClientUrl("~/ESS/Docs/") + "/" + empid + "/"
                       + "/" + servicetype.ToString() + "/" + reqid + "/" + fileInfo.Name;

                    return url1;
                }
                else
                    return "";
            }
            else
                return "";

        }
        public DataTable dtLeaveResult = null;
        void LOAD_VALIDATELEAVE(string empCode)
        {

            var fromDate = Request.Form["ESSfromDate"].ToArabicDate();
            var toDate = Request.Form["ESStoDate"].ToArabicDate();
            var Compcode = user.Login_Company_Code;
            var Leavetype = Request.Form["ddlLeaveType"];
            var StartDay = "N";// ((Request.Form["chkFirst"] =="true")?"Y":"N");

            if (Request.Form["chkIsFirstDayOff"] != null && Request.Form["chkIsFirstDayOff"] == "on")
                StartDay = "Y";

            var EndDay = "N";// ((Request.Form["enddayhalf"] == "true") ? "Y" : "N");

            if (Request.Form["chkIsSecondDayOff"] != null && Request.Form["chkIsSecondDayOff"] == "on")
                EndDay = "Y";

            //var Totdays = Request["leaveDaysTotal1"].ToDouble();


            List<AppCode.Pro_Parameters> parameters = new List<AppCode.Pro_Parameters>();
            parameters.Add(new AppCode.Pro_Parameters { _par_name = "P_COMP_CODE", _par_value = Compcode });
            parameters.Add(new AppCode.Pro_Parameters { _par_name = "P_EMP_ID", _par_value = empCode });
            parameters.Add(new AppCode.Pro_Parameters { _par_name = "P_LEAVE_TYPE", _par_value = Leavetype });
            parameters.Add(new AppCode.Pro_Parameters { _par_name = "P_LEAVE_START_DATE", _par_data_type = "Date", _par_value = fromDate });
            parameters.Add(new AppCode.Pro_Parameters { _par_name = "P_LEAVE_END_DATE", _par_data_type = "Date", _par_value = toDate });
            parameters.Add(new AppCode.Pro_Parameters { _par_name = "P_START_DAY_HALF", _par_value = StartDay });
            parameters.Add(new AppCode.Pro_Parameters { _par_name = "P_END_DAY_HALF", _par_value = EndDay });

            var tot_days = new AppCode.Pro_Parameters { _par_name = "P_TOT_DAYS", _par_value = 0D, _par_in_out = "OUT", _par_data_type = "NUMBER" };
            parameters.Add(tot_days);
            var is_elgible = new AppCode.Pro_Parameters { _par_name = "P_IS_ELIGIBLE", _par_value = "", _par_in_out = "OUT", _par_data_type = "VARCHAR2" };
            parameters.Add(is_elgible);
            var status_leave = new AppCode.Pro_Parameters { _par_name = "P_STATUS_TEXT", _par_value = "", _par_in_out = "OUT", _par_data_type = "VARCHAR2" };
            parameters.Add(status_leave);


            AppCode.dbaccess db = new dbaccess();
            db.executeProcedure("PAY_GET_LEAVE_ELIG_DETAILS", parameters);

            var dt = new DataTable();
            dt.Columns.Add("Total");
            dt.Columns.Add("Elgible");
            dt.Columns.Add("Status");

            DataRow dr = dt.NewRow();
            dr["Total"] = tot_days._par_value;
            dr["Elgible"] = is_elgible._par_value;
            dr["Status"] = status_leave._par_value;

            dt.Rows.Add(dr);
            if (dt.Rows.Count > 0)
            {
                dtLeaveResult = dt;
                var json = JsonConvert.SerializeObject(dt);
                Response.Write(json);
            }
            else
                Response.Write("{}");


        }


        //Attendance
        public string leavehistory = "";
        void PopulateAttHist()
        {

            var empcode = Request.QueryString["EmpCode"];
            var from = Request["From"].ToArabicDate();
            var to = Request["To"].ToArabicDate();
            var dt = objDB.execute_query_retun_datatable(@"
 
SELECT AWH_ROW_ID1 AWH_ROW_ID,APP_CNT,
     CASE WHEN NVL(APP_CNT,0)=0 THEN 'P' ELSE 'C' END  AS WF_STATUS3,
    ELRH_TOT_DAYS,
    PEMP_EMP_NAME, PEMP_EMP_CODE,PBM_BRANCH_NAME,
    LEAVE_REQ_ID, to_char(FROM_DT,'dd/mm/yyyy') as FROM_DT, to_char(TO_DT,'dd/mm/yyyy') as TO_DT, 
    SUBJ, LEAVE_BODY, EST_SERVICE_ID, to_char(ELRH_REQUESTED_DT,'dd/mm/yyyy')Req_date,
    EST_SERVICE_DESC,  CASE WHEN ELRH_REQ_STATUS_ID = 11  THEN 
            --'Waiting For ' || Replace(INITCAP(AEG_DESCRIPTION),'Waiting For','') 
            GET_LEAVE_STATUS (ELRH_REQ_STATUS_ID)     
        ELSE  
            APPROVED_GROUP_NAME
        END   CURR_STATUS_NAME, ATT_REASON_DESC1, 
    ELR_ATT_CORRECTION_REASON, ATT_REASON_DESC, TRAINING_NAME, 
    TRAINING_DESC, ELR_TRAIN_PURPOSE, ELR_TRAIN_GOAL, 
    ES_CODE, ES_DESC,  
    SERVICE_REQ_DESC, PAYSLIP_REQ_DESC, TRAINING_FEEDBACK_DESC,
    PLTM_LEAVE_TYPE_CODE, nvl(PLTM_LEAVE_TYPE_DESC, PLTM_LEAVE_ARABIC_DESC) as PLTM_LEAVE_TYPE_DESC,
        (TO_DT - FROM_DT) +1 as TOT_DAYS,
       CONCAT(CONCAT(APP_DATE,' '),APP_TIME)APP_DATE,ELRH_REQ_STATUS_ID,
           APP_TIME
   
FROM 
    ESS_LEAVE_REQ_DETAILS_ALL,
        ESS_REQ_APPROVED_GROUP,
      ( select AWDD_WF_HEADER_ROW_ID WF_HEADER_ID, SUM(CASE WHEN wd2.AWDD_STATUS in ('A','R') THEN 1 ELSE 0 END) AS APP_CNT
  from AUTH_WF_DOC_DETAIL wd2  
                    GROUP BY AWDD_WF_HEADER_ROW_ID)
    where
             UNIQUE_ID_1 =  TO_CHAR(LEAVE_REQ_ID)  
                    AND UNIQUE_ID_2 = TO_CHAR(LEAVE_REQ_ID) 
    and WF_HEADER_ID(+) = AWH_ROW_ID1
    AND EST_SERVICE_ID ='5'
    and PEMP_EMP_CODE = '" + user.Emp_Code + @"'

and ((FROM_DT BETWEEN to_date('" + from.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + to.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') OR
        TO_DT BETWEEN to_date('" + from.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + to.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy')) OR 
          ( FROM_DT < to_date('" + from.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + to.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') < TO_DT ))
   ORDER BY LEAVE_REQ_ID DESC
");

            //        AND HRH_DOCUMENT_DATE BETWEEN TO_DATE(to_char('" + from + @"') ,'dd/mm/yyyy') AND
            //TO_DATE(to_char('" + to + @"'), 'dd/mm/yyyy')
            if (dt != null && dt.Rows.Count > 0)
            {

                var fname = "";
                dt.Columns.Add("FileName");
                foreach (DataRow dr in dt.Rows)
                {
                    if (dr["LEAVE_REQ_ID"] != "")
                    {
                        fname = Getfilinfo("5", dr["PEMP_EMP_CODE"].ToString(), dr["LEAVE_REQ_ID"].ToString());
                        dr["FileName"] = fname;
                    }

                }
                foreach (DataRow item in dt.Rows)
                {
                    String DeleteString = @"<td class='green'  > <a href='#' onclick=""DeleteReq('" + item["LEAVE_REQ_ID"].ToString() + @"');"">Cancel </a></td>";

                    if (item["WF_STATUS3"].ToString() == "C")
                    {
                        //WF_STATUS3
                        DeleteString = @"<td class='gray' onclick='alert(""Not allowed."");' > Cancel </td>";
                    }
                    leavehistory += @"<tr>
    <td class='f1'><a href='javascript:LoadApprover3(" + item["AWH_ROW_ID"].ToString() + @"," + item["LEAVE_REQ_ID"].ToString() + @")'> " + item["ATT_REASON_DESC"].ToString() + @" </a></td>
<td class='gray'> " + item["FROM_DT"].ToString() + @" </td>
<td class='gray'> " + item["TO_DT"].ToString() + @" </td>
<td class='green'> " + item["TOT_DAYS"].ToString() + @" </td>
" + DeleteString + @"
</tr>

";

                }
                //ddlLeaveType.InnerHtml = ddlLeaves2;



                var JSONresult = JsonConvert.SerializeObject(dt);
                Response.Write(JSONresult);
                // Response.Write(leavehistory);
            }
            else
            {
                Response.Write("{}");
            }



        }


        void PopulateAttapproved()
        {

            var empcode = Request.QueryString["EmpCode"];
            var from = Request["From"].ToArabicDate();
            var to = Request["To"].ToArabicDate();
            var dt = objDB.execute_query_retun_datatable(@"
 
SELECT AWH_ROW_ID1 AWH_ROW_ID,APP_CNT,
     CASE WHEN NVL(APP_CNT,0)=0 THEN 'P' ELSE 'C' END  AS WF_STATUS3,
    ELRH_TOT_DAYS,
    PEMP_EMP_NAME, PEMP_EMP_CODE,PBM_BRANCH_NAME,
    LEAVE_REQ_ID, to_char(FROM_DT,'dd/mm/yyyy') as FROM_DT, to_char(TO_DT,'dd/mm/yyyy') as TO_DT, 
    SUBJ, LEAVE_BODY, EST_SERVICE_ID,  to_char(ELRH_REQUESTED_DT,'dd/mm/yyyy')Req_date,
    EST_SERVICE_DESC,  CASE WHEN ELRH_REQ_STATUS_ID = 11  THEN 
            --'Waiting For ' || Replace(INITCAP(AEG_DESCRIPTION),'Waiting For','') 
            GET_LEAVE_STATUS (ELRH_REQ_STATUS_ID)     
        ELSE  
            APPROVED_GROUP_NAME
        END   CURR_STATUS_NAME, ATT_REASON_DESC1, 
    ELR_ATT_CORRECTION_REASON, ATT_REASON_DESC, TRAINING_NAME, 
    TRAINING_DESC, ELR_TRAIN_PURPOSE, ELR_TRAIN_GOAL, 
    ES_CODE, ES_DESC,  
    SERVICE_REQ_DESC, PAYSLIP_REQ_DESC, TRAINING_FEEDBACK_DESC,
    PLTM_LEAVE_TYPE_CODE, nvl(PLTM_LEAVE_TYPE_DESC, PLTM_LEAVE_ARABIC_DESC) as PLTM_LEAVE_TYPE_DESC,
        (TO_DT - FROM_DT) +1 as TOT_DAYS, CONCAT(CONCAT(APP_DATE,' '),APP_TIME)APP_DATE,APP_TIME
   
FROM 
    ESS_LEAVE_REQ_DETAILS_ALL,
        ESS_REQ_APPROVED_GROUP,
      ( select AWDD_WF_HEADER_ROW_ID WF_HEADER_ID, SUM(CASE WHEN wd2.AWDD_STATUS in ('A','R') THEN 1 ELSE 0 END) AS APP_CNT
  from AUTH_WF_DOC_DETAIL wd2  
                    GROUP BY AWDD_WF_HEADER_ROW_ID)
    where
             UNIQUE_ID_1 =  TO_CHAR(LEAVE_REQ_ID)
                    AND UNIQUE_ID_2 = TO_CHAR(LEAVE_REQ_ID) 
    and WF_HEADER_ID(+) = AWH_ROW_ID1
    AND EST_SERVICE_ID ='5'
    
and exists ( 
    SELECT 1 FROM AUTH_WF_HEADER, AUTH_WF_DOC_DETAIL
              WHERE     
                    AWDD_WF_HEADER_ROW_ID = AWH_ROW_ID
                    AND AWDD_EMP_APPROVED_BY = '" + empcode + @"'
                    AND AWH_UNIQUE_ID1 =  TO_CHAR(LEAVE_REQ_ID)
                    AND AWH_UNIQUE_ID2 = TO_CHAR(LEAVE_REQ_ID)
    )

and ((FROM_DT BETWEEN to_date('" + from.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + to.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') OR
        TO_DT BETWEEN to_date('" + from.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + to.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy')) OR 
          ( FROM_DT < to_date('" + from.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + to.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') < TO_DT ))
  ORDER BY  LEAVE_REQ_ID DESC
");

            //        AND HRH_DOCUMENT_DATE BETWEEN TO_DATE(to_char('" + from + @"') ,'dd/mm/yyyy') AND
            //TO_DATE(to_char('" + to + @"'), 'dd/mm/yyyy')
            if (dt != null && dt.Rows.Count > 0)
            {
                var fname = "";
                dt.Columns.Add("FileName");
                foreach (DataRow dr in dt.Rows)
                {
                    if (dr["LEAVE_REQ_ID"] != "")
                    {
                        fname = Getfilinfo("5", dr["PEMP_EMP_CODE"].ToString(), dr["LEAVE_REQ_ID"].ToString());
                        dr["FileName"] = fname;
                    }

                }
                var JSONresult = JsonConvert.SerializeObject(dt);
                Response.Write(JSONresult);

            }
            else
            {
                Response.Write("{}");
            }



        }

        //Technology
        private void PopulateTechhist()
        {
            var empcode = Request.QueryString["EmpCode"];
            var from = Request["From"].ToArabicDate();
            var to = Request["To"].ToArabicDate();
            //'Waiting for ' || AEG_DESCRIPTION  CURR_STATUS_NAME, 
            var dt = db.execute_query_retun_datatable(@"

SELECT  
   CASE WHEN NVL(APP_CNT,0)=0 THEN 'P' ELSE 'C' END  AS WF_STATUS3,
    ELRH_TOT_DAYS,
    PEMP_EMP_NAME, PEMP_EMP_CODE,
    LEAVE_REQ_ID, to_char(FROM_DT,'dd/mm/yyyy') as FROM_DT, to_char(TO_DT,'dd/mm/yyyy') as TO_DT, 
    SUBJ, LEAVE_BODY, EST_SERVICE_ID, 
    EST_SERVICE_DESC, ATT_REASON_DESC1, ELRH_REQ_STATUS_ID,
    ELR_ATT_CORRECTION_REASON, ATT_REASON_DESC, TRAINING_NAME, 
    TRAINING_DESC, ELR_TRAIN_PURPOSE, ELR_TRAIN_GOAL, 
    ES_CODE, ES_DESC,to_char(ELRH_REQUESTED_DT,'dd/mm/yyyy')REQ_DATE,  
    SERVICE_REQ_DESC, PAYSLIP_REQ_DESC, TRAINING_FEEDBACK_DESC,
    PLTM_LEAVE_TYPE_CODE, nvl(PLTM_LEAVE_TYPE_DESC, PLTM_LEAVE_ARABIC_DESC) as PLTM_LEAVE_TYPE_DESC,
       CASE WHEN ELRH_REQ_STATUS_ID = 11  THEN 
            --'Waiting For ' || Replace(INITCAP(AEG_DESCRIPTION),'Waiting For','') 
            GET_LEAVE_STATUS (ELRH_REQ_STATUS_ID)     
        ELSE  
            APPROVED_GROUP_NAME
        END   CURR_STATUS_NAME, APP_CNT, CONCAT(CONCAT(APP_DATE,' '),APP_TIME)APP_DATE,APP_TIME
   FROM 
    ESS_LEAVE_REQ_DETAILS_ALL, ESS_REQ_APPROVED_GROUP,
      ( select AWDD_WF_HEADER_ROW_ID WF_HEADER_ID, SUM(CASE WHEN wd2.AWDD_STATUS in ('A','R') THEN 1 ELSE 0 END) AS APP_CNT
  from AUTH_WF_DOC_DETAIL wd2  
                    GROUP BY AWDD_WF_HEADER_ROW_ID)
    where
        to_char(LEAVE_REQ_ID) =to_char(UNIQUE_ID_1(+)) 
        AND to_char(LEAVE_REQ_ID) = to_char(UNIQUE_ID_2(+))
    AND EST_SERVICE_ID ='7'
    AND ES_CATEGORY_ID ='TECH'
    and PEMP_EMP_CODE = '" + user.Emp_Code + @"'
      and WF_HEADER_ID(+) = AWH_ROW_ID1

 
and ((FROM_DT BETWEEN to_date('" + from.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + to.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') OR
        TO_DT BETWEEN to_date('" + from.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + to.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy')) OR 
          ( FROM_DT < to_date('" + from.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + to.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') < TO_DT ))
 

 order by LEAVE_REQ_ID desc
 
");


            if (dt != null && dt.Rows.Count > 0)
            {

                foreach (DataRow item in dt.Rows)
                {
                    String DeleteString = @"<td class='green'  > <a href='#' onclick=""DeleteReq('" + item["LEAVE_REQ_ID"].ToString() + @"');"">Cancel </a></td>";

                    if (item["WF_STATUS3"].ToString() == "C")
                    {
                        //WF_STATUS3
                        DeleteString = @"<td class='gray' onclick='alert(""Not allowed."");' > Cancel </td>";
                    }
                    leavehistory += @"<tr>
    <td class='f1'> <a href='javascript:LoadApprover3(" + item["est_service_id"].ToString() + @"," + item["LEAVE_REQ_ID"].ToString() + @")'>" + item["SERVICE_REQ_DESC"].ToString() + @"</a> </td>
<td class='gray'> " + item["FROM_DT"].ToString() + @" </td>
<td class='green'> " + item["CURR_STATUS_NAME"].ToString() + @" </td>
" + DeleteString + @"
</tr>

";

                }
                //ddlLeaveType.InnerHtml = ddlLeaves2;

                var JSONresult = JsonConvert.SerializeObject(dt);
                Response.Write(JSONresult);
            }
            else
            {
                Response.Write("{}");
            }


        }

        private void PopulateTechapproved()
        {
            var empcode = Request["EmpCode"];
            var from = Request["From"].ToArabicDate();
            var to = Request["To"].ToArabicDate();
            //'Waiting for ' || AEG_DESCRIPTION  CURR_STATUS_NAME, 
            var dt = db.execute_query_retun_datatable(@"

SELECT  
   CASE WHEN NVL(APP_CNT,0)=0 THEN 'P' ELSE 'C' END  AS WF_STATUS3,
    ELRH_TOT_DAYS,
    PEMP_EMP_NAME, PEMP_EMP_CODE,PBM_BRANCH_NAME,
    LEAVE_REQ_ID, to_char(FROM_DT,'dd/mm/yyyy') as FROM_DT, to_char(TO_DT,'dd/mm/yyyy') as TO_DT, 
    SUBJ, LEAVE_BODY, EST_SERVICE_ID, 
    EST_SERVICE_DESC, ATT_REASON_DESC1, 
    ELR_ATT_CORRECTION_REASON, ATT_REASON_DESC, TRAINING_NAME, 
    TRAINING_DESC, ELR_TRAIN_PURPOSE, ELR_TRAIN_GOAL, 
    ES_CODE, ES_DESC, to_char(ELRH_REQUESTED_DT,'dd/mm/yyyy')REQ_DATE, 
    SERVICE_REQ_DESC, PAYSLIP_REQ_DESC, TRAINING_FEEDBACK_DESC,
    PLTM_LEAVE_TYPE_CODE, nvl(PLTM_LEAVE_TYPE_DESC, PLTM_LEAVE_ARABIC_DESC) as PLTM_LEAVE_TYPE_DESC,
       CASE WHEN ELRH_REQ_STATUS_ID = 11  THEN 
            --'Waiting For ' || Replace(INITCAP(AEG_DESCRIPTION),'Waiting For','') 
            GET_LEAVE_STATUS (ELRH_REQ_STATUS_ID)     
        ELSE  
            APPROVED_GROUP_NAME
        END   CURR_STATUS_NAME, APP_CNT, CONCAT(CONCAT(APP_DATE,' '),APP_TIME)APP_DATE,APP_TIME
   FROM 
    ESS_LEAVE_REQ_DETAILS_ALL, ESS_REQ_APPROVED_GROUP,
      ( select AWDD_WF_HEADER_ROW_ID WF_HEADER_ID, SUM(CASE WHEN wd2.AWDD_STATUS in ('A','R') THEN 1 ELSE 0 END) AS APP_CNT
  from AUTH_WF_DOC_DETAIL wd2  
                    GROUP BY AWDD_WF_HEADER_ROW_ID)
    where
        to_char(LEAVE_REQ_ID) =to_char(UNIQUE_ID_1(+)) 
        AND to_char(LEAVE_REQ_ID) = to_char(UNIQUE_ID_2(+))
    AND EST_SERVICE_ID ='7'
    AND ES_CATEGORY_ID ='TECH'
   -- and PEMP_EMP_CODE = '" + empcode + @"'
      and WF_HEADER_ID(+) = AWH_ROW_ID1

 
and ((FROM_DT BETWEEN to_date('" + from.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + to.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') OR
        TO_DT BETWEEN to_date('" + from.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + to.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy')) OR 
          ( FROM_DT < to_date('" + from.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + to.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') < TO_DT ))
 
 and exists ( 
    SELECT 1 FROM AUTH_WF_HEADER, AUTH_WF_DOC_DETAIL
              WHERE     
                    AWDD_WF_HEADER_ROW_ID = AWH_ROW_ID
                    AND AWDD_EMP_APPROVED_BY = '" + user.Emp_Code + @"'
                    AND AWH_UNIQUE_ID1 =  TO_CHAR(LEAVE_REQ_ID)
                    AND AWH_UNIQUE_ID2 = TO_CHAR(LEAVE_REQ_ID)
    )

 order by LEAVE_REQ_ID desc
 
");


            if (dt != null && dt.Rows.Count > 0)
            {

                var JSONresult = JsonConvert.SerializeObject(dt);
                Response.Write(JSONresult);
            }
            else
            {
                Response.Write("{}");
            }


        }

        private void PrintLetterUpdate()
        {
            var module = Request["MODULE"];
            var letter = Request["LETTER"];
            var year = Request["ddlYear"]; var month = Request["ddlMonth"];
            int rowid = objDB.execute_scalar(@"SELECT PPT_LETTER_LOG_SEQ.NEXTVAL FROM DUAL").ToInt();
            string lettercode = objDB.execute_scalar(@"SELECT AL_LETTER_CODE FROM AMM_LETTERS WHERE AL_LETTER_NAME='Salary Letter'");
            string strquery = @"INSERT INTO PPT_LETTER_LOG(BAO_HEADER_ROWD_ID,BAO_LETTER_CODE,BAO_EMP_CODE,BAO_DATE,BAO_USER_ID,BAO_PRINT_DATE)
                 VALUES(" + rowid + @",'" + lettercode + @"','" + user.Emp_Code.ToUpper() + @"',sysdate,'" + user.User_Id + @"',sysdate)";
            var cnt = db.execute_query(strquery);


        }

        //admin history

        private void Populateadminhist()
        {
            var empcode = Request["EmpCode"];
            var from = Request["From"].ToArabicDate();
            var to = Request["To"].ToArabicDate();
            var dt = db.execute_query_retun_datatable(@"

SELECT   
   CASE WHEN NVL(APP_CNT,0)=0 THEN 'P' ELSE 'C' END  AS WF_STATUS3,
    ELRH_TOT_DAYS,
    PEMP_EMP_NAME, PEMP_EMP_CODE,PBM_BRANCH_NAME,
    LEAVE_REQ_ID, to_char(FROM_DT,'dd/mm/yyyy') as FROM_DT, to_char(TO_DT,'dd/mm/yyyy') as TO_DT, 
    SUBJ, LEAVE_BODY, EST_SERVICE_ID, ELRH_REQ_STATUS_ID,
    EST_SERVICE_DESC, ATT_REASON_DESC1, 
    ELR_ATT_CORRECTION_REASON, ATT_REASON_DESC, TRAINING_NAME, 
    TRAINING_DESC, ELR_TRAIN_PURPOSE, ELR_TRAIN_GOAL, 
    ES_CODE, ES_DESC, to_char(ELRH_REQUESTED_DT,'dd/mm/yyyy')REQ_DATE, 
    SERVICE_REQ_DESC, PAYSLIP_REQ_DESC, TRAINING_FEEDBACK_DESC,
    PLTM_LEAVE_TYPE_CODE, nvl(PLTM_LEAVE_TYPE_DESC, PLTM_LEAVE_ARABIC_DESC) as PLTM_LEAVE_TYPE_DESC,
       CASE WHEN ELRH_REQ_STATUS_ID = 11  THEN 
            --'Waiting For ' || Replace(INITCAP(AEG_DESCRIPTION),'Waiting For','') 
            GET_LEAVE_STATUS (ELRH_REQ_STATUS_ID)     
        ELSE  
            APPROVED_GROUP_NAME
        END   CURR_STATUS_NAME, APP_CNT, CONCAT(CONCAT(APP_DATE,' '),APP_TIME)APP_DATE,APP_TIME
   FROM 
    ESS_LEAVE_REQ_DETAILS_ALL, ESS_REQ_APPROVED_GROUP,
      ( select AWDD_WF_HEADER_ROW_ID WF_HEADER_ID, SUM(CASE WHEN wd2.AWDD_STATUS in ('A','R') THEN 1 ELSE 0 END) AS APP_CNT
  from AUTH_WF_DOC_DETAIL wd2  
                    GROUP BY AWDD_WF_HEADER_ROW_ID)
    where
        to_char(LEAVE_REQ_ID) =to_char(UNIQUE_ID_1(+)) 
        AND to_char(LEAVE_REQ_ID) = to_char(UNIQUE_ID_2(+))
    AND EST_SERVICE_ID ='7'
    AND ES_CATEGORY_ID ='ADMIN'
    and PEMP_EMP_CODE = '" + user.Emp_Code + @"'
      and WF_HEADER_ID(+) = AWH_ROW_ID1

 
and ((FROM_DT BETWEEN to_date('" + from.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + to.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') OR
        TO_DT BETWEEN to_date('" + from.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + to.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy')) OR 
          ( FROM_DT < to_date('" + from.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + to.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') < TO_DT ))
 order by LEAVE_REQ_ID desc
");



            if (dt != null && dt.Rows.Count > 0)
            { 
                var JSONresult = JsonConvert.SerializeObject(dt);
                Response.Write(JSONresult);
            }
            else
            {
                Response.Write("{}");
            }


        }


        private void Populateadminapproved()
        {
            var empcode = Request["EmpCode"];
            var from = Request["From"].ToArabicDate();
            var to = Request["To"].ToArabicDate();
            var dt = db.execute_query_retun_datatable(@"

SELECT   
   CASE WHEN NVL(APP_CNT,0)=0 THEN 'P' ELSE 'C' END  AS WF_STATUS3,
    ELRH_TOT_DAYS,
    PEMP_EMP_NAME, PEMP_EMP_CODE,PBM_BRANCH_NAME,
    LEAVE_REQ_ID, to_char(FROM_DT,'dd/mm/yyyy') as FROM_DT, to_char(TO_DT,'dd/mm/yyyy') as TO_DT, 
    SUBJ, LEAVE_BODY, EST_SERVICE_ID, 
    EST_SERVICE_DESC, ATT_REASON_DESC1, 
    ELR_ATT_CORRECTION_REASON, ATT_REASON_DESC, TRAINING_NAME, 
    TRAINING_DESC, ELR_TRAIN_PURPOSE, ELR_TRAIN_GOAL, 
    ES_CODE, ES_DESC, to_char(ELRH_REQUESTED_DT,'dd/mm/yyyy')REQ_DATE, 
    SERVICE_REQ_DESC, PAYSLIP_REQ_DESC, TRAINING_FEEDBACK_DESC,
    PLTM_LEAVE_TYPE_CODE, nvl(PLTM_LEAVE_TYPE_DESC, PLTM_LEAVE_ARABIC_DESC) as PLTM_LEAVE_TYPE_DESC,
       CASE WHEN ELRH_REQ_STATUS_ID = 11  THEN 
            --'Waiting For ' || Replace(INITCAP(AEG_DESCRIPTION),'Waiting For','') 
            GET_LEAVE_STATUS (ELRH_REQ_STATUS_ID)     
        ELSE  
            APPROVED_GROUP_NAME
        END   CURR_STATUS_NAME, APP_CNT, CONCAT(CONCAT(APP_DATE,' '),APP_TIME)APP_DATE,APP_TIME
   FROM 
    ESS_LEAVE_REQ_DETAILS_ALL, ESS_REQ_APPROVED_GROUP,
      ( select AWDD_WF_HEADER_ROW_ID WF_HEADER_ID, SUM(CASE WHEN wd2.AWDD_STATUS in ('A','R') THEN 1 ELSE 0 END) AS APP_CNT
  from AUTH_WF_DOC_DETAIL wd2  
                    GROUP BY AWDD_WF_HEADER_ROW_ID)
    where
        to_char(LEAVE_REQ_ID) =to_char(UNIQUE_ID_1(+)) 
        AND to_char(LEAVE_REQ_ID) = to_char(UNIQUE_ID_2(+))
    AND EST_SERVICE_ID ='7'
    AND ES_CATEGORY_ID ='ADMIN'
   -- and PEMP_EMP_CODE = '" + user.Emp_Code + @"'
      and WF_HEADER_ID(+) = AWH_ROW_ID1
   and exists ( 
    SELECT 1 FROM AUTH_WF_HEADER, AUTH_WF_DOC_DETAIL
              WHERE     
                    AWDD_WF_HEADER_ROW_ID = AWH_ROW_ID
                    AND AWDD_EMP_APPROVED_BY = '" + user.Emp_Code + @"'
                    AND AWH_UNIQUE_ID1 =  TO_CHAR(LEAVE_REQ_ID)
                    AND AWH_UNIQUE_ID2 = TO_CHAR(LEAVE_REQ_ID)
    )

 
and ((FROM_DT BETWEEN to_date('" + from.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + to.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') OR
        TO_DT BETWEEN to_date('" + from.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + to.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy')) OR 
          ( FROM_DT < to_date('" + from.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + to.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') < TO_DT ))
 order by LEAVE_REQ_ID desc
");



            if (dt != null && dt.Rows.Count > 0)
            {
                var JSONresult = JsonConvert.SerializeObject(dt);
                Response.Write(JSONresult);
            }
            else
            {
                Response.Write("{}");
            }


        }

        //Payslip
        private void PopulateLetterhist()
        {
            var empcode = Request["EmpCode"];
            var from = Request["From"].ToArabicDate();
            var to = Request["To"].ToArabicDate();
            var q = @"
 
SELECT distinct AWH_ROW_ID1 AWH_ROW_ID,APP_CNT,
   CASE WHEN NVL(APP_CNT,0)=0 THEN 'P' ELSE 'C' END  AS WF_STATUS3,
ELRH_TOT_DAYS,
    PEMP_EMP_NAME, PEMP_EMP_CODE,
    LEAVE_REQ_ID, to_char(FROM_DT,'dd/mm/yyyy') as FROM_DT, to_char(TO_DT,'dd/mm/yyyy') as TO_DT, 
    SUBJ, LEAVE_BODY, EST_SERVICE_ID, to_char(ELRH_REQUESTED_DT,'dd/mm/yyyy')REQ_DATE,
    EST_SERVICE_DESC,
        CASE WHEN ELRH_REQ_STATUS_ID = 11  THEN 
            --'Waiting For ' || Replace(INITCAP(AEG_DESCRIPTION),'Waiting For','') 
            GET_LEAVE_STATUS (ELRH_REQ_STATUS_ID)     
        ELSE  
            APPROVED_GROUP_NAME
        END   CURR_STATUS_NAME, ATT_REASON_DESC1, ELRH_REQ_STATUS_ID,
    ES_CODE, ES_DESC,  
    SERVICE_REQ_DESC, PAYSLIP_REQ_DESC, TRAINING_FEEDBACK_DESC,
    PLTM_LEAVE_TYPE_CODE, nvl(PLTM_LEAVE_TYPE_DESC, PLTM_LEAVE_ARABIC_DESC) as PLTM_LEAVE_TYPE_DESC,
   STATUS,lr.ELR_LEAVE_REQ_HEAD_ID, ELR_LETTER_TO_BANK, ELR_LETTER_YYYYMM, ELR_LETTER_TYPE,
  l. AL_LETTER_NAME,TO_CHAR (ELR_APPROVED_DT, 'DD-MM-YYY HH24:MI') APP_DATE,APP_TIME
FROM 
    ESS_LEAVE_REQ_DETAILS_ALL, 
    ESS_LEAVE_REQ   lr ,AMM_LETTERS l,
          ESS_REQ_APPROVED_GROUP,
      ( select AWDD_WF_HEADER_ROW_ID WF_HEADER_ID, SUM(CASE WHEN wd2.AWDD_STATUS in ('A','R') THEN 1 ELSE 0 END) AS APP_CNT
  from AUTH_WF_DOC_DETAIL wd2  
                    GROUP BY AWDD_WF_HEADER_ROW_ID)
    where
 to_char(LEAVE_REQ_ID) = to_char(UNIQUE_ID_1(+))
       AND to_char(LEAVE_REQ_ID) = to_char(UNIQUE_ID_2  (+))
       AND EST_SERVICE_ID ='13'
       and ELR_LEAVE_REQ_HEAD_ID = LEAVE_REQ_ID 
       and AL_LETTER_CODE = ELR_LETTER_TYPE
       and PEMP_EMP_CODE ='" + user.Emp_Code + @"'

         and WF_HEADER_ID(+) = AWH_ROW_ID1

and ((FROM_DT BETWEEN to_date('" + from.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + to.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') OR
        TO_DT BETWEEN to_date('" + from.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + to.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy')) OR 
          ( FROM_DT < to_date('" + from.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + to.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') < TO_DT ))
   order by LEAVE_REQ_ID desc
 
";
            log_error.write_to_log_file("letterhistory", "false", q);
            var dt = db.execute_query_retun_datatable(q);
            //AppCode.LogError.WriteToLogFile("", "", q);

            if (dt != null && dt.Rows.Count > 0)
            {


                dt.Columns.Add("FileName"); dt.Columns.Add("FileNameHr");
                foreach (DataRow dr in dt.Rows)
                {
                    if (dr["LEAVE_REQ_ID"].ToString() != "")
                    {
                        dr["FileName"] = Getfilinfo("13", dr["PEMP_EMP_CODE"].ToString(), dr["LEAVE_REQ_ID"].ToString());
                        dr["FileNameHr"] = GetfilinfoHR("13", dr["PEMP_EMP_CODE"].ToString(), dr["LEAVE_REQ_ID"].ToString());
                    }

                }

                var JSONresult = JsonConvert.SerializeObject(dt);
                Response.Write(JSONresult);
            }
            else
            {
                Response.Write("{}");
            }


        }


        // Travel

        private void PopulateTravelhist()
        {
            //ELR_LEAVE_REQ_HEAD_ID
            var empcode = Request["EmpCode"];
            var from = Request["From"].ToArabicDate();
            var to = Request["To"].ToArabicDate();
            string serviceType = "11";
            var q = @"

SELECT  distinct  ELR_LEAVE_REQ_HEAD_ID LEAVE_REQ_ID, CASE WHEN NVL(APP_CNT,0)=0 THEN 'P' ELSE 'C' END AS WF_STATUS3, ELR_REQ_STATUS_ID,ELR_LEAVE_REQ_HEAD_ID,
       HTH_ROW_ID, HTH_DOCUMENT_NO, HTH_DOCUMENT_SYSID, 
       HTH_DOCUMENT_DATE, HTH_REF_ROW_ID, HTH_EMP_CODE, 
       HTH_VENUE_DETAILS, to_char(HTH_START_DATE,'dd/mm/yyyy') HTH_START_DATE, to_char(HTH_END_DATE,'dd/mm/yyyy') as HTH_END_DATE, 
       HTH_NO_OF_DAYS, HTH_PER_DIEM_DAYS, HTH_TIMING_FROM, 
       HTH_TIMING_TO, HTH_TOUR_AGENDA, HTH_POSTED_YN, 
       HTH_STATUS, HTH_WF_STATUS,'' as Status2,HTH_ROW_ID,
       to_char(ELR_REQUESTED_DT,'dd/mm/yyyy')REQ_DATE,
       CASE WHEN ELR_REQ_STATUS_ID = 11  THEN 
            --'Waiting For ' || Replace(INITCAP(AEG_DESCRIPTION),'Waiting For','') 
            GET_LEAVE_STATUS (ELR_REQ_STATUS_ID)     
        ELSE  
            APPROVED_GROUP_NAME
        END   Status1, 
    APP_CNT,TO_CHAR (ELR_APPROVED_DT, 'DD-MM-YYY hh24:mi') APP_DATE,
            TO_CHAR (ELR_APPROVED_DT, 'hh24:mi')APP_TIME
    
FROM HRT_TRAVEL_HEADER, ESS_LEAVE_REQ,
    ESS_REQ_APPROVED_GROUP,
    ( select AWDD_WF_HEADER_ROW_ID WF_HEADER_ID, SUM(CASE WHEN wd2.AWDD_STATUS in ('A','R') THEN 1 ELSE 0 END) AS APP_CNT
  from AUTH_WF_DOC_DETAIL wd2  
                    GROUP BY AWDD_WF_HEADER_ROW_ID)
where
     HTH_DOCUMENT_NO = ELR_REF_DOC_NO
  --  and AWDD_ROW_ID = AWH_CURRENT_CONFIG_ROW_ID
   -- AND to_char(ELR_LEAVE_REQ_HEAD_ID) = to_char(AWH_UNIQUE_ID1) AND to_char(ELR_LEAVE_REQ_HEAD_ID) = to_char(AWH_UNIQUE_ID2)
   -- and AEG_EMP_GROUP_ID=AWDD_GROUP_ID
     AND HTH_EMP_CODE = '" + empcode + @"'
    --AND ELR_REQ_STATUS_ID = 3
    AND WF_HEADER_ID(+)  = AWH_ROW_ID1
    and ELR_SERVICE_TYPE = 11 
         AND UNIQUE_ID_1 =  TO_CHAR(ELR_LEAVE_REQ_HEAD_ID)
                    AND UNIQUE_ID_2 = TO_CHAR(ELR_LEAVE_REQ_HEAD_ID) 
    
and ((HTH_START_DATE BETWEEN to_date('" + from.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + to.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') OR
        HTH_END_DATE BETWEEN to_date('" + from.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + to.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy')) OR 
          ( HTH_START_DATE < to_date('" + from.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + to.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') < HTH_END_DATE ))
order by ELR_LEAVE_REQ_HEAD_ID desc
     
";
            //AppCode.LogError.WriteToLogFile("", false, q);

            var dt = db.execute_query_retun_datatable(q);


            var JSONresult = JsonConvert.SerializeObject(dt);
            Response.Write(JSONresult);

        }


        private void PopulateTravelapproved()
        {
            //ELR_LEAVE_REQ_HEAD_ID
            var empcode = Request["EmpCode"];
            var from = Request["From"].ToArabicDate();
            var to = Request["To"].ToArabicDate();


            //SELECT distinct ELR_LEAVE_REQ_HEAD_ID LEAVE_REQ_ID, CASE WHEN NVL(APP_CNT,0)=0 THEN 'P' ELSE 'C' END AS WF_STATUS3, ELR_REQ_STATUS_ID,ELR_LEAVE_REQ_HEAD_ID,
            //       HTH_ROW_ID, HTH_DOCUMENT_NO, HTH_DOCUMENT_SYSID, 
            //       HTH_DOCUMENT_DATE, HTH_REF_ROW_ID, HTH_EMP_CODE, ELR_EMP_ID,ENAME,DEPT_NAME,BRANCH_NAME,
            //       HTH_VENUE_DETAILS, to_char(HTH_START_DATE,'dd/mm/yyyy') HTH_START_DATE, to_char(HTH_END_DATE,'dd/mm/yyyy') as HTH_END_DATE, 
            //       HTH_NO_OF_DAYS, HTH_PER_DIEM_DAYS, HTH_TIMING_FROM, 
            //       HTH_TIMING_TO, HTH_TOUR_AGENDA, HTH_POSTED_YN, to_char(ELR_REQUESTED_DT,'dd/mm/yyyy')REQ_DATE,
            //       HTH_STATUS, HTH_WF_STATUS,'Waiting for ' || AEG_DESCRIPTION as Status1,HTH_ROW_ID,
            //  TO_CHAR (ELR_APPROVED_DT, 'DD-MM-YYY hh24:mi') APP_DATE,
            //            TO_CHAR (ELR_APPROVED_DT, 'hh24:mi')APP_TIME
            //FROM HRT_TRAVEL_HEADER, ESS_LEAVE_REQ,VU_EMP_DET,
            //    AUTH_WF_HEADER, AUTH_WF_DOC_DETAIL,AUTH_EMP_GROUP,
            //    ( select AWDD_WF_HEADER_ROW_ID WF_HEADER_ID, SUM(CASE WHEN wd2.AWDD_STATUS in ('A','R') THEN 1 ELSE 0 END) AS APP_CNT
            //  from AUTH_WF_DOC_DETAIL wd2  
            //                    GROUP BY AWDD_WF_HEADER_ROW_ID)
            //where
            //    HTH_DOCUMENT_NO = ELR_REF_DOC_NO
            //    and AWDD_ROW_ID = AWH_CURRENT_CONFIG_ROW_ID
            //    AND to_char(ELR_LEAVE_REQ_HEAD_ID) = to_char(AWH_UNIQUE_ID1) AND to_char(ELR_LEAVE_REQ_HEAD_ID) = to_char(AWH_UNIQUE_ID2)
            //    and AEG_EMP_GROUP_ID=AWDD_GROUP_ID   
            //    AND ELR_REQ_STATUS_ID = 3
            //    AND WF_HEADER_ID(+)  = AWH_ROW_ID
            //    and ELR_SERVICE_TYPE = 11 and ELR_EMP_ID=EMPID
            //and ((HTH_START_DATE BETWEEN to_date('" + from.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + to.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') OR
            //        HTH_END_DATE BETWEEN to_date('" + from.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + to.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy')) OR 
            //          ( HTH_START_DATE < to_date('" + from.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + to.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') < HTH_END_DATE ))
            //and exists ( 
            //    SELECT 1 FROM AUTH_WF_HEADER, AUTH_WF_DOC_DETAIL
            //              WHERE     
            //                    AWDD_WF_HEADER_ROW_ID = AWH_ROW_ID
            //                    AND AWDD_EMP_APPROVED_BY = '" + empcode + @"'
            //                    AND AWH_UNIQUE_ID1 =  TO_CHAR(ELR_LEAVE_REQ_HEAD_ID)
            //                    AND AWH_UNIQUE_ID2 = TO_CHAR(ELR_LEAVE_REQ_HEAD_ID)
            //    )

            //union all

            var q = @"
SELECT DISTINCT ELR_LEAVE_REQ_HEAD_ID LEAVE_REQ_ID, 'C' AS WF_STATUS3, ELR_REQ_STATUS_ID,ELR_LEAVE_REQ_HEAD_ID,
       HTH_ROW_ID, HTH_DOCUMENT_NO, HTH_DOCUMENT_SYSID, 
       HTH_DOCUMENT_DATE, HTH_REF_ROW_ID, HTH_EMP_CODE,ELR_EMP_ID,ENAME,DEPT_NAME,BRANCH_NAME, 
       HTH_VENUE_DETAILS, to_char(HTH_START_DATE,'dd/mm/yyyy') HTH_START_DATE, to_char(HTH_END_DATE,'dd/mm/yyyy') as HTH_END_DATE, 
       HTH_NO_OF_DAYS, HTH_PER_DIEM_DAYS, HTH_TIMING_FROM, 
       HTH_TIMING_TO, HTH_TOUR_AGENDA, HTH_POSTED_YN,  to_char(ELR_REQUESTED_DT,'dd/mm/yyyy') REQ_DATE,
       HTH_STATUS, HTH_WF_STATUS,ELS_STATUS_NAME as Status1,HTH_ROW_ID ,TO_CHAR (ELR_APPROVED_DT, 'DD-MM-YYY hh24:mi') APP_DATE,
            TO_CHAR (ELR_APPROVED_DT, 'hh24:mi')APP_TIME
FROM HRT_TRAVEL_HEADER, ESS_LEAVE_REQ,ESS_LEAVE_REQ_STAT_MST,VU_EMP_DET,
    ( select AWDD_WF_HEADER_ROW_ID WF_HEADER_ID, SUM(CASE WHEN wd2.AWDD_STATUS in ('A','R') THEN 1 ELSE 0 END) AS APP_CNT
  from AUTH_WF_DOC_DETAIL wd2  
                    GROUP BY AWDD_WF_HEADER_ROW_ID)
 where
    HTH_DOCUMENT_NO = ELR_REF_DOC_NO   
    and ELR_REQ_STATUS_ID <> '3'
    and ELS_STATUS_ID =ELR_REQ_STATUS_ID
    and ELR_SERVICE_TYPE = 11  and ELR_EMP_ID=EMPID
and exists ( 
    SELECT 1 FROM AUTH_WF_HEADER, AUTH_WF_DOC_DETAIL
              WHERE     
                    AWDD_WF_HEADER_ROW_ID = AWH_ROW_ID
                    AND AWDD_EMP_APPROVED_BY = '" + empcode + @"'
                    AND AWH_UNIQUE_ID1 =  TO_CHAR(ELR_LEAVE_REQ_HEAD_ID)
                    AND AWH_UNIQUE_ID2 = TO_CHAR(ELR_LEAVE_REQ_HEAD_ID)
    )
and ((HTH_START_DATE BETWEEN to_date('" + from.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + to.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') OR
        HTH_END_DATE BETWEEN to_date('" + from.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + to.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy')) OR 
          ( HTH_START_DATE < to_date('" + from.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + to.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') < HTH_END_DATE ))
order by ELR_LEAVE_REQ_HEAD_ID desc
";
           AppCode.LogError.WriteToLogFile("", false, q);

            var dt = db.execute_query_retun_datatable(q);


            var JSONresult = JsonConvert.SerializeObject(dt);
            Response.Write(JSONresult);

        }


        void LOAD_MY_ManagerDetails(string empCode)
        {
            DBAccess ObjDb = new DBAccess();
            string str_select = @" SELECT ed.PEMP_EMP_NAME_AR,CASE ed.PEMP_FAMILYSTAT_YN WHEN 'Y' THEN 'YES' ELSE 'NO' END PEMP_FAMILYSTAT_YN, ed.PEMP_REPORTING_TO,MNGR.PEMP_EMP_NAME PEMP_REPORTING_TO_NAME, ED.PEMP_IS_RELIGION, ed.PEMP_MANPOWER_NUMBER, ed.PEMP_CIVIL_NO, ed.PEMP_SUB_GRADE,  
            (SELECT PSGD_SUB_GRADE_DESC FROM PPM_SUB_GRADE_MASTER where PSGD_SUB_GRADE_CODE =ed.PEMP_SUB_GRADE ) as PSGD_SUB_GRADE_DESC, ";
            str_select += " ED.PEMP_EMP_COMPANY_CODE,ED.PEMP_EMP_CODE, ED.PEMP_EMP_NAME, ED.PEMP_EMP_FIRST_NAME, ED.PEMP_EMP_MIDDLE_NAME, ED.PEMP_EMP_FAMILY_NAME, ED.PEMP_EMP_SHORT_NAME,  ";
            str_select += " ED.PEMP_EMP_ADDRESS_1, ED.PEMP_EMP_ADDRESS_2, ED.PEMP_EMP_ADDRESS_3, ED.PEMP_EMP_PHONE, ED.PEMP_EMAIL_ADDRESS ,TO_CHAR(ED.PEMP_EMP_DATE_OF_BIRTH,'dd/MM/yyyy')PEMP_EMP_DATE_OF_BIRTH, ";
            str_select += " ED.PEMP_EMP_BRANCH_CODE,DM.PDM_DIV_CODE PEMP_EMP_DIV_CODE,  ";
            str_select += " ED.PEMP_EMP_DEPTARTMENT_CODE, ED.PEMP_EMP_LCDSG_CODE,ED.PEMP_EMP_DESIGNATION_CODE, ED.PEMP_EMP_GRADE_CODE, ";
            str_select += " ED.PEMP_EMP_COUNTRY, ED.PEMP_EMP_SECTOR, ED.PEMP_EMP_SOCIAL_STATUS,CASE  ED.PEMP_EMP_ACTIVE WHEN 'Y' THEN 'YES' ELSE 'NO' END PEMP_EMP_ACTIVE, ED. PEMP_EMP_PHOTO,CASE ED.PEMP_RELIGION_CODE WHEN 'ISM' THEN 'ISLAMIC' WHEN 'HIN' THEN 'HINDU' WHEN 'CHR' THEN 'CHRISTIAN' END PEMP_RELIGION_CODE, CD.ACD_COMP_NAME, BM.PBM_BRANCH_NAME, DM.PDM_DIV_NAME,  ";
            str_select += " DPM.PDPM_DEPARTMENT_DESC,DSM.PDSM_DESIGNATION_DESC AS LABOUR_CARD_DESIGNATION, DSM.PDSM_DESIGNATION_DESC, GM.PGDM_GRADE_DESC, CM.ACM_COUNTRY_NAME,   ";
            str_select += " SM.PSCM_SECTOR_NAME, ED.PEMP_EMP_NEXT_TO_KIN, ED.PEMP_EMP_NKT_RELATION, ED.PEMP_EMP_NKT_ADDRESS_1, ED.PEMP_EMP_NKT_ADDRESS_2, ED.PEMP_EMP_NKT_ADDRESS_3, ED.PEMP_EMP_NKT_PHONE,     ";
            str_select += " ED.PEMP_EMP_NAME_TITLE,CASE ED.PEMP_EMP_SEX WHEN 'M' THEN 'MALE' ELSE 'FEMALE' END PEMP_EMP_SEX, ED.PEMP_EMP_SOCIAL_STATUS, ED.PEMP_EMP_TYPE, to_char(ED.PEMP_EMP_JOIN_DATE,'dd/MM/yyyy')PEMP_EMP_JOIN_DATE, ED.PEMP_EMP_CLASS, ED.PEMP_EMP_PLACE_OF_BIRTH,    ";
            str_select += " ED.PEMP_EMP_SOCIAL_SECURITY, ED.PEMP_EMP_SOCIAL_SECURITY_NO, ED.PEMP_EMP_REMARKS, ED.PEMP_EMP_LOCAL ,   ";
            str_select += " TO_CHAR(ED.PEMP_LAST_ACCESSED_DATE , 'dd/MM/yyyy') AS PEMP_LAST_ACCESSED_DATE ,CASE ED.PEMP_EMP_PAY_MODE WHEN 'BTR' THEN 'BANK TRANSFER' WHEN 'CAS' THEN 'CASH' WHEN 'CHQ' THEN 'CHEQUE'END PEMP_EMP_PAY_MODE, ";

            str_select += "ED.PEMP_PERSONAL_PHONE,ED.PEMP_PERSONAL_MAIL,";
            str_select += " ED.PEMP_EMP_GRATUITY_FLAG,to_char(ED.PEMP_EMP_GRATUITY_BEGIN_DATE,'dd/mm/yyyy') as PEMP_EMP_GRATUITY_BEGIN_DATE,ED.PEMP_FIRST_GRATUITY_DAYS,ED.PEMP_SECOND_GRATUITY_DAYS,ED.PEMP_SECOND_AFTER_YEARS, ED.PEMP_EMP_NOTICE_PERIOD,ED.PEMP_ER_NOTICE_PERIOD,to_char(ED.PEMP_NOTICE_PER_START_DATE , 'dd/MM/yyyy') AS NOTICE_PER_START_DATE ,to_char(ED.PEMP_NOTICE_PER_END_DATE , 'dd/MM/yyyy') AS NOTICE_PER_END_DATE,ED.PEMP_ON_NOTICE_PERIOD,to_char(ED.PEMP_PROBATION_PERIOD , 'dd/MM/yyyy')PEMP_PROBATION_PERIOD, ";
            str_select += " to_char(ED.PEMP_LEAVE_ACCRUAL_START_DATE , 'dd/MM/yyyy')PEMP_LEAVE_ACCRUAL_START_DATE,ED.PEMP_LEAVE_CYCLE_FREQUENCY,ED.PEMP_LEAVE_CYCLE_FREQUENCY_BY,ED.PEMP_EMP_CLASS,ED.PEMP_MAXIMUM_DEPENDENTS,ED.PEMP_EMP_TICKETS_ADULT,ED.PEMP_EMP_TICKETS_CHILD,";
            str_select += " ED.PEMP_ANN_LEAVE_INDICATOR,ED.PEMP_NO_TAS,ED.PEMP_DAY_WAGES_FLAG,ED.PEMP_EMP_TERM_PROCESSED_IND,ED.PEMP_EMP_OT_ELIGIBLE_FLAG,";
            str_select += " ED.PEMP_DEFAULT_SPONSOR,GET_SPONSOR_NAME(ED.PEMP_DEFAULT_SPONSOR) SPN_NAME,ED.PEMP_AIR_FARE,ED.PEMP_VISA_CHRG,ED.PEMP_OTHER_CHRG,ED.PEMP_ACTING_MNGR,mgact.PEMP_EMP_NAME PEMP_REPORTING_ACT_NAME";


            str_select += " FROM AMM_COMPANY_DETAILS CD, PPM_EMPLOYEE_DETAILS ED,PPM_EMPLOYEE_DETAILS mngr, PPM_EMPLOYEE_DETAILS mgact,PPM_BRANCH_MASTER BM,PPM_DIVISION_MASTER DM, PPM_DEPARTMENT_MASTER DPM,    ";
            str_select += " PPM_DESIGNATION_MASTER DSG, PPM_DESIGNATION_MASTER DSM, PPM_GRADE_MASTER GM,AMM_COUNTRY_MASTER CM,    ";
            str_select += " PPM_SECTOR_MASTER SM WHERE ED.PEMP_EMP_BRANCH_CODE = BM.PBM_BRANCH_CODE (+)AND   ";
            str_select += " CD.ACD_COMP_CODE = ED.PEMP_EMP_COMPANY_CODE (+)AND   ";
            str_select += " ED.PEMP_EMP_DIV_CODE = DM.PDM_DIV_CODE (+) AND ED.PEMP_EMP_LCDSG_CODE = DSG.PDSM_DESIGNATION_CODE (+) AND   ";
            str_select += "  ED.PEMP_EMP_DEPTARTMENT_CODE = DPM.PDPM_DEPARTMENT_CODE (+) AND ED.PEMP_EMP_DESIGNATION_CODE =   ";
            str_select += "   DSM.PDSM_DESIGNATION_CODE (+) AND ED.PEMP_EMP_GRADE_CODE = GM.PGDM_GRADE_CODE  (+) ";
            str_select += " AND ED.PEMP_EMP_COUNTRY= CM.ACM_COUNTRY_CODE (+) AND ED.PEMP_EMP_SECTOR = SM.PSCM_SECTOR_CODE (+)";
            str_select += "  and ED.PEMP_EMP_COUNTRY = PSCM_SECTOR_COUNTRY (+)   AND ed.PEMP_EMP_CODE='" + empCode + "'";

            str_select += " and ED.PEMP_REPORTING_TO = mngr.pemp_EMP_CODE(+)   ";
            str_select += " and ED.PEMP_ACTING_MNGR = mgact.pemp_EMP_CODE(+)   ";
            AppCode.log_error.write_to_log_file("", "", str_select);

            DataTable dt1 = objDB.execute_query_retun_datatable(str_select);


            if (dt1 != null && dt1.Rows.Count > 0)
            {
                var JSONresult = JsonConvert.SerializeObject(dt1);
                Response.Write(JSONresult);
            }
            else
            {
                Response.Write("{}");
            }
        }

        void LOAD_MY_COMPOFF( )
        {
            EssCompoffDA essCompoffDA = new EssCompoffDA();
            essCompoffDA.LOAD_MY_COMPOFF(Request["EmpCode"]);
        }
        void LOAD_MY_LEAVE_CARRYFOWARD(string empCode)
        {
            DBAccess ObjDb = new DBAccess();

            var fromDate1 = Request["FromDate"].ToArabicDate();
            var toDate1 = Request["ToDate"].ToArabicDate();
            DataTable dt1 = objDB.execute_query_retun_datatable(@"
SELECT ELR_LEAVE_REQ_HEAD_ID,
    ELR_EMP_ID, ELR_LEAVE_ID, to_char(ELR_FROM_DT,'dd/mm/yyyy') ELR_FROM_DT1, 
    to_char(ELR_TO_DT,'dd/mm/yyyy') ELR_TO_DT1, ELR_TOT_DAYS, ELR_BODY, 
    ELR_REQ_STATUS_ID, to_char(ELR_REQUESTED_DT,'dd/mm/yyyy')REQ_DATE,
    EMPID, ENAME,PLTM_LEAVE_TYPE_DESC,
    BRANCH_NAME, DEPT_NAME, DIV_NAME, DESIGNATION_DESC,APPROVED_GROUP_NAME,
    APP_CNT,TO_CHAR (ELR_APPROVED_DT, 'DD-MM-YYY hh24:mi') APP_DATE,
            TO_CHAR (ELR_APPROVED_DT, 'hh24:mi')APP_TIME
    
FROM 
    ESS_LEAVE_REQ,ppm_leave_type_master,
    vu_emp_det,ESS_REQ_APPROVED_GROUP,
      ( select AWDD_WF_HEADER_ROW_ID WF_HEADER_ID, SUM(CASE WHEN wd2.AWDD_STATUS in ('A','R') THEN 1 ELSE 0 END) AS APP_CNT
  from AUTH_WF_DOC_DETAIL wd2  
                    GROUP BY AWDD_WF_HEADER_ROW_ID)
where
    empid=elr_emp_id
    and ELR_SERVICE_TYPE= 15
    AND ELR_LEAVE_ID = PLTM_LEAVE_TYPE_CODE
     AND UNIQUE_ID_1 =  TO_CHAR(ELR_LEAVE_REQ_HEAD_ID)
                    AND UNIQUE_ID_2 = TO_CHAR(ELR_LEAVE_REQ_HEAD_ID) 
    and WF_HEADER_ID(+) = AWH_ROW_ID1
    
 AND ELR_EMP_ID = '" + empCode + @"'

 and ((ELR_FROM_DT BETWEEN to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') OR
        ELR_TO_DT BETWEEN to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy')) OR 
          ( ELR_FROM_DT < to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') < ELR_TO_DT ))
   order by ELR_LEAVE_REQ_HEAD_ID desc
");
            if (dt1 != null && dt1.Rows.Count > 0)
            {
                var JSONresult = JsonConvert.SerializeObject(dt1);
                Response.Write(JSONresult);
            }
            else
            {
                Response.Write("{}");
            }
        }
        void LOAD_APPROVED_LETTER_HISTORY_BY_ME(string empCode)
        {
           var from = Request["From"].ToArabicDate();
            var to = Request["To"].ToArabicDate();

            DBAccess ObjDb = new DBAccess();

            var Q = @"

SELECT ELR_LEAVE_REQ_HEAD_ID,    ELR_EMP_ID, ELR_LEAVE_ID, to_char(ELR_REQUESTED_DT,'dd/mm/yyyy') ELR_REQUESTED_DT, 
    to_char(ELR_TO_DT,'dd/mm/yyyy') ELR_TO_DT1, ELR_TOT_DAYS, ELR_BODY, 
    ELR_REQ_STATUS_ID, 
    EMPID, ENAME,
    BRANCH_NAME, DEPT_NAME, DIV_NAME, DESIGNATION_DESC,
   ELR_LETTER_TO_BANK, ELR_LETTER_YYYYMM, ELR_LETTER_TYPE ,AL_LETTER_NAME,
    APPROVED_GROUP_NAME,TO_CHAR (ELR_APPROVED_DT, 'DD-MM-YYY hh24:mi') APP_DATE,
            TO_CHAR (ELR_APPROVED_DT, 'hh24:mi')APP_TIME
FROM 
    ESS_LEAVE_REQ,
    vu_emp_det, AMM_LETTERS  , ESS_REQ_APPROVED_GROUP
where
    empid=elr_emp_id
    and ELR_SERVICE_TYPE=13
   and AL_LETTER_CODE = ELR_LETTER_TYPE
  AND UNIQUE_ID_1 =  TO_CHAR(ELR_LEAVE_REQ_HEAD_ID)
                    AND UNIQUE_ID_2 = TO_CHAR(ELR_LEAVE_REQ_HEAD_ID) 
    and exists ( 
    SELECT 1 FROM AUTH_WF_HEADER, AUTH_WF_DOC_DETAIL
              WHERE     
                    AWDD_WF_HEADER_ROW_ID = AWH_ROW_ID
                    AND AWDD_EMP_APPROVED_BY = '" + empCode + @"'
                    AND AWH_UNIQUE_ID1 =  TO_CHAR(ELR_LEAVE_REQ_HEAD_ID)
                    AND AWH_UNIQUE_ID2 = TO_CHAR(ELR_LEAVE_REQ_HEAD_ID)
    )

AND  ELR_REQUESTED_DT between to_date('" + from.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + to.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy')
 
ORDER BY ELR_LEAVE_REQ_HEAD_ID DESC
";
           log_error.write_to_log_file("letterapproved", "false", Q);
            DataTable dt1 = objDB.execute_query_retun_datatable(Q);

            
            if (dt1 != null && dt1.Rows.Count > 0)
            {

                dt1.Columns.Add("FileName");
                dt1.Columns.Add("FileNameHR");
                foreach (DataRow dr in dt1.Rows)
                {
                    if (dr["ELR_LEAVE_REQ_HEAD_ID"].ToString() != "")
                    {
                        dr["FileName"] = Server.UrlEncode(Getfilinfo("13", dr["ELR_EMP_ID"].ToString(), dr["ELR_LEAVE_REQ_HEAD_ID"].ToString()));

                        dr["FileNameHR"] = Server.UrlEncode(GetfilinfoHR("13", dr["ELR_EMP_ID"].ToString(), dr["ELR_LEAVE_REQ_HEAD_ID"].ToString()));
                    }

                }


                var JSONresult = JsonConvert.SerializeObject(dt1);
                
               Response.Write(JSONresult);
            }
            else
            {
                Response.Write("{}");
            }
        }
        void CARRYFORWARD_APPROVED_BY_ME(string empCode)
        {

            var fromDate1 = Request["FromDate"].ToArabicDate();
            var toDate1 = Request["ToDate"].ToArabicDate();
            DBAccess ObjDb = new DBAccess();
            string strquery = @"

SELECT ELR_LEAVE_REQ_HEAD_ID,
    ELR_EMP_ID, ELR_LEAVE_ID, to_char(ELR_FROM_DT,'dd/mm/yyyy') ELR_FROM_DT1, 
    to_char(ELR_TO_DT,'dd/mm/yyyy') ELR_TO_DT1, ELR_TOT_DAYS, ELR_BODY, 
    ELR_REQ_STATUS_ID, 
    EMPID, ENAME,PLTM_LEAVE_TYPE_DESC,to_char(ELR_REQUESTED_DT,'dd/mm/yyyy')REQ_DATE,
    BRANCH_NAME, DEPT_NAME, DIV_NAME, DESIGNATION_DESC,APPROVED_GROUP_NAME,
TO_CHAR (ELR_APPROVED_DT, 'DD-MM-YYY hh24:mi') APP_DATE,
            TO_CHAR (ELR_APPROVED_DT, 'hh24:mi')APP_TIME
    
FROM 
    ESS_LEAVE_REQ,ppm_leave_type_master,
    vu_emp_det,ESS_REQ_APPROVED_GROUP
where
    empid=elr_emp_id
     and ELR_SERVICE_TYPE= 15
    AND ELR_LEAVE_ID = PLTM_LEAVE_TYPE_CODE
     AND UNIQUE_ID_1 =  TO_CHAR(ELR_LEAVE_REQ_HEAD_ID)
                    AND UNIQUE_ID_2 = TO_CHAR(ELR_LEAVE_REQ_HEAD_ID) 
    
    and exists ( 
    SELECT 1 FROM AUTH_WF_HEADER, AUTH_WF_DOC_DETAIL
              WHERE     
                    AWDD_WF_HEADER_ROW_ID = AWH_ROW_ID
                    AND AWDD_EMP_APPROVED_BY = '" + empCode + @"'
                    AND AWH_UNIQUE_ID1 =  TO_CHAR(ELR_LEAVE_REQ_HEAD_ID)
                    AND AWH_UNIQUE_ID2 = TO_CHAR(ELR_LEAVE_REQ_HEAD_ID)
    )
and ((ELR_FROM_DT BETWEEN to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') OR
        ELR_TO_DT BETWEEN to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy')) OR 
          ( ELR_FROM_DT < to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') < ELR_TO_DT ))
   ORDER BY ELR_LEAVE_REQ_ID DESC  ";

            //AppCode.LogError.WriteToLogFile("", false, strquery);
            DataTable dt1 = objDB.execute_query_retun_datatable(strquery);
            if (dt1 != null && dt1.Rows.Count > 0)
            {
                var JSONresult = JsonConvert.SerializeObject(dt1);
                Response.Write(JSONresult);
            }
            else
            {
                Response.Write("{}");
            }
        }
        
        void LOADLEAVEBALANCELEAVEPLANNER(string empCode)
        {

            var fromDate1 = Request["FromDate"].ToArabicDate();
            var toDate1 = Request["ToDate"].ToArabicDate();
            DBAccess ObjDb = new DBAccess();
            string strquery = @"

SELECT 
    PLTM_LEAVE_TYPE_DESC ,PLTM_LEAVE_TYPE_CODE,  PLTM_IS_LEAVE_IN_HOURS ,
    PELD_LEAVE_AVAILED_DAYS, nvl(PLTM_SHOWIN_LVBALANCE,'Y') as PLTM_SHOWIN_LVBALANCE,PELD_LEAVE_DAYS
FROM 
    PPM_LEAVE_TYPE_MASTER, PPM_EMPLOYEE_LEAVE_DETAILS 
WHERE
    nvl(PLTM_VISIBLE,'N') ='Y' and NVL(PLTM_SHOWIN_LVPLANNER,'N')='Y'  and
    PLTM_LEAVE_TYPE_CODE = PELD_LEAVE_TYPE and 
     PELD_CODE='" + empCode + @"' AND 
     PLTM_COMP_CODE   = '01'  ORDER BY PLTM_SORT_ORDER";



            //AppCode.LogError.WriteToLogFile("", false, strquery);
            DataTable dt1 = objDB.execute_query_retun_datatable(strquery);
            if (dt1 != null && dt1.Rows.Count > 0)
            {
                var JSONresult = JsonConvert.SerializeObject(dt1);
                Response.Write(JSONresult);
            }
            else
            {
                Response.Write("{}");
            }
        }
        void LOADLEAVEBALACEAPPROVEDBYME(string empCode)
        {

            var fromDate1 = Request["FromDate"].ToArabicDate();
            var toDate1 = Request["ToDate"].ToArabicDate();
            DBAccess ObjDb = new DBAccess();
            string strquery = @"
 
 SELECT T.* FROM
(SELECT 
    PLTM_LEAVE_TYPE_DESC ,PLTM_LEAVE_TYPE_CODE,  PLTM_IS_LEAVE_IN_HOURS ,
    PELD_LEAVE_AVAILED_DAYS, nvl(PLTM_SHOWIN_LVBALANCE,'Y') as PLTM_SHOWIN_LVBALANCE,PELD_LEAVE_DAYS,SUM(ELR_TOT_DAYS)WAITINGFORAPP, PLTM_SORT_ORDER
FROM 
    PPM_LEAVE_TYPE_MASTER, PPM_EMPLOYEE_LEAVE_DETAILS,ESS_LEAVE_REQ 
WHERE
    nvl(PLTM_VISIBLE,'N') ='Y'    and
    PLTM_LEAVE_TYPE_CODE = PELD_LEAVE_TYPE and 
     PELD_CODE='" + empCode + @"' AND    ELR_SERVICE_TYPE(+)=1 AND
     PELD_CODE=ELR_EMP_ID(+) and PLTM_LEAVE_TYPE_CODE=ELR_LEAVE_ID(+) and ELR_REQ_STATUS_ID(+)=3 AND
     PLTM_COMP_CODE   = '01'  GROUP BY  PLTM_LEAVE_TYPE_DESC ,PLTM_LEAVE_TYPE_CODE,  PLTM_IS_LEAVE_IN_HOURS ,
    PELD_LEAVE_AVAILED_DAYS,  PLTM_SHOWIN_LVBALANCE,PELD_LEAVE_DAYS,PLTM_SORT_ORDER)T ORDER BY PLTM_SORT_ORDER";

           

            //AppCode.LogError.WriteToLogFile("", false, strquery);
            DataTable dt1 = objDB.execute_query_retun_datatable(strquery);
            if (dt1 != null && dt1.Rows.Count > 0)
            {
                var JSONresult = JsonConvert.SerializeObject(dt1);
                Response.Write(JSONresult);
            }
            else
            {
                Response.Write("{}");
            }
        }

        void LOADLEAVEHISTORYAPPROVEDBYME(string empCode)
        {

            var fromDate1 = Request["FromDate"].ToArabicDate();
            var toDate1 = Request["ToDate"].ToArabicDate();
            DBAccess ObjDb = new DBAccess();
            string strquery = @"

SELECT ELR_LEAVE_REQ_HEAD_ID,
    ELR_EMP_ID, ELR_LEAVE_ID, to_char(ELR_FROM_DT,'dd/mm/yyyy') ELR_FROM_DT1, 
    to_char(ELR_TO_DT,'dd/mm/yyyy') ELR_TO_DT1, ELR_TOT_DAYS, ELR_BODY, 
    ELR_REQ_STATUS_ID, 
    EMPID, ENAME,PLTM_LEAVE_TYPE_DESC,to_char(ELR_REQUESTED_DT,'dd/mm/yyyy')REQ_DATE, 
    BRANCH_NAME, DEPT_NAME, DIV_NAME, DESIGNATION_DESC,APPROVED_GROUP_NAME,
    TO_CHAR (ELR_APPROVED_DT, 'DD-MM-YYY hh24:mi') APP_DATE,
            TO_CHAR (ELR_APPROVED_DT, 'hh24:mi')APP_TIME
    
FROM 
    ESS_LEAVE_REQ,ppm_leave_type_master,
    vu_emp_det,ESS_REQ_APPROVED_GROUP
where
    empid=elr_emp_id
     and ELR_SERVICE_TYPE= 1
    AND ELR_LEAVE_ID = PLTM_LEAVE_TYPE_CODE
     AND UNIQUE_ID_1 =  TO_CHAR(ELR_LEAVE_REQ_HEAD_ID)
                    AND UNIQUE_ID_2 = TO_CHAR(ELR_LEAVE_REQ_HEAD_ID) 
    
    and exists ( 
    SELECT 1 FROM AUTH_WF_HEADER, AUTH_WF_DOC_DETAIL
              WHERE     
                    AWDD_WF_HEADER_ROW_ID = AWH_ROW_ID
                    AND AWDD_EMP_APPROVED_BY = '" + empCode + @"'
                    AND AWH_UNIQUE_ID1 =  TO_CHAR(ELR_LEAVE_REQ_HEAD_ID)
                    AND AWH_UNIQUE_ID2 = TO_CHAR(ELR_LEAVE_REQ_HEAD_ID)
    )
and ((ELR_FROM_DT BETWEEN to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') OR
        ELR_TO_DT BETWEEN to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy')) OR 
          ( ELR_FROM_DT < to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') < ELR_TO_DT ))
    ORDER BY ELR_LEAVE_REQ_ID DESC ";

            //AppCode.LogError.WriteToLogFile("", false, strquery);
            DataTable dt1 = objDB.execute_query_retun_datatable(strquery);
            if (dt1 != null && dt1.Rows.Count > 0)
            {
                var fname = "";
                dt1.Columns.Add("FileName");
                foreach (DataRow dr in dt1.Rows)
                {
                    if (dr["ELR_LEAVE_REQ_HEAD_ID"] != "")
                    {
                        fname = Getfilinfo("1", dr["ELR_EMP_ID"].ToString(), dr["ELR_LEAVE_REQ_HEAD_ID"].ToString());
                        dr["FileName"] = fname;
                    }

                }
                var JSONresult = JsonConvert.SerializeObject(dt1);
                Response.Write(JSONresult);
            }
            else
            {
                Response.Write("{}");
            }
        }


        
       void LOADGETCHECK_TIME_BALANCE(string empCode)
        {

            var fromDate1 = Request["FromDate"].ToArabicDate().ToString("MMyyyy");
            var PermissionCode = Request["PermissionCode"];
            DBAccess ObjDb = new DBAccess();
            string strquery = @"
WITH CTE
     AS (SELECT TRUNC ( (DIFF) * 24) || '.' || MOD ( (DIFF) * 24 * 60, 60)
                   tot_applied,               
               TRUNC(DIFF * 24 * 60 , 5)  to_app_mins
           FROM (SELECT SUM (DIFF) DIFF
                   FROM (SELECT TO_DATE (ELR_TRAIN_PURPOSE,
                                         'DD-MM-YYYY hh24:mi')
                                   SDATE,
                                TO_DATE (ELR_TRAIN_GOAL,
                                         'DD-MM-YYYY hh24:mi')
                                   EDATE,
                                  TO_DATE (ELR_TRAIN_GOAL,
                                           'DD-MM-YYYY hh24:mi')
                                - TO_DATE (ELR_TRAIN_PURPOSE,
                                           'DD-MM-YYYY hh24:mi')
                                   AS DIFF,
                                TO_DATE (ELR_TRAIN_PURPOSE,
                                         'DD-MM-YYYY hh24:mi')
                                   AS APPLY_DATE,
                                ELR_EMP_ID,
                                ENAME,
                                ELR_TRAIN_PURPOSE,
                                ELR_TRAIN_GOAL,
                                SUBSTR (ELR_TRAIN_GOAL, 11, 6) TTIME,
                                SUBSTR (ELR_TRAIN_PURPOSE, 11, 6) FTIME
                           FROM ESS_LEAVE_REQ, VU_EMP_DET
                          WHERE     ELR_EMP_ID = EMPID
                                AND ELR_SERVICE_TYPE IN (7, 8, 3)
                                AND ELR_REQ_STATUS_ID IN (3, 7, 8) -- AND ELR_LEAVE_REQ_HEAD_ID=10568
                                AND ELR_EMP_ID='" + empCode+ @"'
                                 AND ELR_LEAVE_ID='" + PermissionCode + @"'                                 )
                  WHERE TO_CHAR (APPLY_DATE, 'MMYYYY') = '" + fromDate1 + @"'  )) --and permi=''
                                                                
SELECT CTE.*,
       PPT_PERMISSION_CODE,
       PPT_PERMISSION_DESC,PPT_ALLOWPAST_DAY,
       CASE
          WHEN PPT_MAX_ALLOWED_IN_MONTH_MINS > 0
          THEN
                TO_CHAR ( (PPT_MAX_ALLOWED_IN_MONTH_MINS / 60), '00')
             || '.'
             || TRIM (
                   TO_CHAR (MOD (PPT_MAX_ALLOWED_IN_MONTH_MINS, 60), '00'))
          ELSE
             ''
       END
          AS PPT_MAX_ALLOWED_IN_MONTH_MINS,
       CASE WHEN PPT_MAX_ALLOWED_IN_MONTH_MINS > 0 THEN 'Y' ELSE 'N' END
          AS SHOWIN_LVBALANCE,
       TRUNC (SYSDATE) + PPT_MAX_ALLOWED_IN_MONTH_MINS / 24 / 60 AS dddd,
       TRUNC (SYSDATE) + to_app_mins / 24 / 60 AS tot_app_date,
case when nvl(to_app_mins,0) <> 0 and PPT_MAX_ALLOWED_IN_MONTH_MINS>0  then  
          TRUNC (
               (PPT_MAX_ALLOWED_IN_MONTH_MINS - to_app_mins)/ 60
               )
       || '.'
       || MOD (
             TRUNC (
                  (PPT_MAX_ALLOWED_IN_MONTH_MINS - to_app_mins) ),
             60)
 else         CASE
          WHEN PPT_MAX_ALLOWED_IN_MONTH_MINS > 0 then   TO_CHAR ( (PPT_MAX_ALLOWED_IN_MONTH_MINS / 60), '00')
             || '.'
             || TRIM (
                   TO_CHAR (MOD (PPT_MAX_ALLOWED_IN_MONTH_MINS, 60), '00'))  end end
          AS BALANCE
  FROM CTE, PPM_PERMISSION_TYPES
 WHERE PPT_MAX_ALLOWED_IN_MONTH_MINS > 0  and PPT_PERMISSION_CODE='" + PermissionCode+"'";

            AppCode.LogError.WriteToLogFile("", false, strquery);
            DataTable dt1 = objDB.execute_query_retun_datatable(strquery);
            var PPT_ALLOWPAST_DAY = ObjDb.execute_scalar("select PPT_ALLOWPAST_DAY from PPM_PERMISSION_TYPES r WHERE PPT_PERMISSION_CODE='"+ PermissionCode + @"'");
            if (PPT_ALLOWPAST_DAY != "Y")
            {
                var FromDate = Request["FromDate"].ToArabicDate();
                if (FromDate.Date < DateTime.Now.Date)
                {
                    WriteErrorMessage("You cant apply permission for past days");
                    return;
                }
            }

            if (dt1 != null && dt1.Rows.Count > 0)
            {
 				
                var JSONresult = JsonConvert.SerializeObject(dt1);
                Response.Write(JSONresult);
            }
            else
            {
                Response.Write("{}");
            }
        }
        void LOAD_LEAVE_APPLIED_BY_ME(string empCode)
        {

            var fromDate1 = Request["FromDate"].ToArabicDate();
            var toDate1 = Request["ToDate"].ToArabicDate();
            DBAccess ObjDb = new DBAccess();
            string strquery = @"

SELECT ELR_LEAVE_REQ_HEAD_ID,
    ELR_EMP_ID, ELR_LEAVE_ID, to_char(ELR_FROM_DT,'dd/mm/yyyy') ELR_FROM_DT1, 
    to_char(ELR_TO_DT,'dd/mm/yyyy') ELR_TO_DT1, ELR_TOT_DAYS, ELR_BODY, 
    ELR_REQ_STATUS_ID, 
    EMPID, ENAME,PLTM_LEAVE_TYPE_DESC,to_char(ELR_REQUESTED_DT,'dd/mm/yyyy')REQ_DATE, 
    BRANCH_NAME, DEPT_NAME, DIV_NAME, DESIGNATION_DESC,APPROVED_GROUP_NAME
    
FROM 
    ESS_LEAVE_REQ,PPM_LEAVE_TYPE_MASTER,
    vu_emp_det,ESS_REQ_APPROVED_GROUP
where
    empid=elr_emp_id
    and elr_apply_emp_code ='" + empCode + @"'
     and ELR_SERVICE_TYPE= 1
    AND ELR_LEAVE_ID = PLTM_LEAVE_TYPE_CODE
     AND UNIQUE_ID_1 =  TO_CHAR(ELR_LEAVE_REQ_HEAD_ID)
                    AND UNIQUE_ID_2 = TO_CHAR(ELR_LEAVE_REQ_HEAD_ID) 
    
    
and ((ELR_FROM_DT BETWEEN to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') OR
        ELR_TO_DT BETWEEN to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy')) OR 
          ( ELR_FROM_DT < to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') < ELR_TO_DT ))
    ORDER BY ELR_LEAVE_REQ_ID DESC ";

            //AppCode.LogError.WriteToLogFile("", false, strquery);
            DataTable dt1 = objDB.execute_query_retun_datatable(strquery);
            if (dt1 != null && dt1.Rows.Count > 0)
            {
                var JSONresult = JsonConvert.SerializeObject(dt1);
                Response.Write(JSONresult);
            }
            else
            {
                Response.Write("{}");
            }
        }


        void LoadGetAllEmployees(string isactive)
        {


            DBAccess ObjDb = new DBAccess();
            string strquery = "";

            if (user.IsAdministrator)
            {
                strquery = @"     SELECT * FROM  V_EMPLOYEE_DETAILS WHERE  PEMP_EMP_COMPANY_CODE='01'  ";
            }
            else
            {
                strquery = @"     SELECT * FROM  V_EMPLOYEE_DETAILS WHERE  PEMP_EMP_COMPANY_CODE='01'  
and exists ( select 1 from ESS_APPLY_FOR_OTHERS where  EAFO_EMP_CODE = PEMP_eMP_cODE and  EAFO_MANAGER_CODE ='" + user.Emp_Code + @"')
";

            }
            if (isactive == "Y")
                strquery += @" and PEMP_EMP_ACTIVE ='Y'";
            else if (isactive == "N")
                strquery += @" and PEMP_EMP_ACTIVE ='N'";

            strquery += @" ORDER BY PEMP_EMP_CODE"; // ORDER BY TO_NUMBER(REGEXP_SUBSTR(PEMP_EMP_CODE, '^[[:digit:]]*'))";
            AppCode.LogError.WriteToLogFile("List of Employees", false, strquery);
            DataTable dt1 = objDB.execute_query_retun_datatable(strquery);
            if (dt1 != null && dt1.Rows.Count > 0)
            {
                var JSONresult = JsonConvert.SerializeObject(dt1);
                Response.Write(JSONresult);
            }
            else
            {
                Response.Write("{}");
            }
        }

        void LoadGetlinemanger(string empid)
        {

            DBAccess ObjDb = new DBAccess();
            string strquery = @"
    SELECT * FROM  V_EMPLOYEE_DETAILS WHERE REPORTINGTO_ID='" + empid + @"' AND PEMP_EMP_COMPANY_CODE='01' and PEMP_EMP_ACTIVE='Y'
ORDER BY  PEMP_EMP_CODE  ";  //TO_NUMBER(REGEXP_REPLACE(PEMP_EMP_CODE, '[^0-9]+', '')) ASC

            AppCode.LogError.WriteToLogFile("LoadGetlinemanger", false, strquery);
            DataTable dt1 = objDB.execute_query_retun_datatable(strquery);
            if (dt1 != null && dt1.Rows.Count > 0)
            {
                var JSONresult = JsonConvert.SerializeObject(dt1);
                Response.Write(JSONresult);
            }
            else
            {
                Response.Write("{}");
            }
        }

        void LoadGetAssetDetails(string empid)
        {

            DBAccess ObjDb = new DBAccess();
            string strquery = @"     SELECT HRT_AST_EMP_CODE,PEMP_EMP_NAME,ROW_NUMBER() OVER(ORDER BY HRT_AST_TAG)SNO,HRT_AST_ID,HRT_AST_TAG,HRT_AST_GROUP_CODE,AAGM_GROUP_DESC,HRT_AST_CAT_CODE,ACM_CAT_DESC,
 HRT_AST_DESC,HRT_AST_REMARKS,to_char(HRT_AST_ISSUE_DATE,'dd/mm/yyyy')HRT_AST_ISSUE_DATE,to_char(HRT_AST_RETURN_DATE,'dd/mm/yyyy')HRT_AST_RETURN_DATE,HRT_AST_STATUS FROM
 HRT_ASSET_EMPLOYEE,PPM_EMPLOYEE_DETAILS,HRT_ASSET_MASTER,AST_ASSET_GROUP_MASTER,AST_CATEGORY_MASTER
 WHERE HRT_AST_EMP_CODE = PEMP_EMP_CODE
 AND HRT_AST_ID = HRT_ASMS_ID(+)
 AND HRT_AST_GROUP_CODE = AAGM_GROUP_CODE 
AND HRT_AST_CAT_CODE = ACM_CAT_CODE   AND HRT_AST_EMP_CODE='" + empid + @"'
ORDER BY HRT_AST_TAG ";

            //AppCode.LogError.WriteToLogFile("", false, strquery);
            DataTable dt1 = objDB.execute_query_retun_datatable(strquery);
            if (dt1 != null && dt1.Rows.Count > 0)
            {
                var JSONresult = JsonConvert.SerializeObject(dt1);
                Response.Write(JSONresult);
            }
            else
            {
                Response.Write("{}");
            }
        }
        void LoadGetActingmanger(string empid)
        {

            DBAccess ObjDb = new DBAccess();
            string strquery = @"
    SELECT * FROM  V_EMPLOYEE_DETAILS WHERE PEMP_ACTING_MNGR='" + empid + @"' AND PEMP_EMP_COMPANY_CODE='01' and PEMP_EMP_ACTIVE='Y' ORDER BY  PEMP_EMP_CODE ";

            //AppCode.LogError.WriteToLogFile("", false, strquery);
            DataTable dt1 = objDB.execute_query_retun_datatable(strquery);
            if (dt1 != null && dt1.Rows.Count > 0)
            {
                var JSONresult = JsonConvert.SerializeObject(dt1);
                Response.Write(JSONresult);
            }
            else
            {
                Response.Write("{}");
            }
        }
        void LOADCOMPOFFHISTORYAPPROVEDBYME()
        {
            EssCompoffDA essCompoffDA = new EssCompoffDA();
            essCompoffDA.LOADCOMPOFFHISTORYAPPROVEDBYME(Request["EmpCode"]);
        }
        void GET_SALARY_BANK(string empCode)
        {
            DBAccess ObjDb = new DBAccess();

           var strdata=@"SELECT 
    PEBD_EMP_CODE, PEBD_BANK_CODE, PEBD_BANK_BRANCH, 
   PEBD_BANK_ACCOUNT_NO, PEBD_BANK_SALARY_CERT_ISSUES, PEBD_BANK_SALARY_TRNS_LETTER,
 ( SELECT COUNT( *) FROM AUTH_EMP_GROUP_MAP,AUTH_EMP_GROUP WHERE AEG_DESCRIPTION='HR MANAGER' and AEGM_EMP_ID='" + empCode + @"' )CNTEMP
FROM PPT_EMPLOYEE_BANK_DETAILS where 
pebd_emp_code='" + empCode + @"'";


            if (ConfigurationManager.AppSettings["INSTALLED_COMPANY"] == "AHP")
                strdata += " and PEBD_BANK_SALARY_CERT_ISSUES='Y'";
               else
                strdata +=  " and PEBD_BANK_SALARY_TRNS_LETTER='Y'";

                DataTable dt1 = objDB.execute_query_retun_datatable(strdata);
            if (dt1 != null && dt1.Rows.Count > 0)
            {
                var JSONresult = JsonConvert.SerializeObject(dt1);
                Response.Write(JSONresult);

            }

            else
            {
                Response.Write("{}");
            }
        }
        private void LEAVE_BALANCE(string empCode, string LeaveId)
        {
            DBAccess ObjDb = new DBAccess();

            var lang = "en-US";
            DataTable dt = ObjDb.execute_query_retun_datatable(@"
 
    SELECT  
    PLTM_LEAVE_TYPE_CODE,
         " + (lang == "en-US" ? "InitCap(PLTM_LEAVE_TYPE_DESC) as PLTM_LEAVE_TYPE_DESC, " : " NVL(PLTM_LEAVE_ARABIC_DESC, PLTM_LEAVE_TYPE_DESC)  AS PLTM_LEAVE_TYPE_DESC, ") + @" 
          CASE
             WHEN PELD_LEAVE_TYPE = 'SL' THEN  PELD_LEAVE_AVAILED_DAYS --NVL (PELD_CURR_AVAIL_DAYS, 0)
             ELSE PELD_LEAVE_AVAILED_DAYS
          END
             AS BALANCE_DAYS,
          PELD_LEAVE_DAYS AS LEAVE_DAYS,SUM(ELR_TOT_DAYS)WAITINGFORAPP,
        to_char(PELD_LEAVE_PROCESSED_DATE,'dd/MM/yyyy') as PELD_LEAVE_PROCESSED_DATE,
        to_char( PELD_LEAVE_PROCESSED_DATE + PELD_LEAVE_AFTER_DAYS ,'dd/MM/yyyy') as   TO_DATE,
    PEMP_EMP_CODE, PEMP_EMP_NAME,PLTM_SORT_ORDER
 FROM
    V_EMPLOYEE_DETAILS,
    PPM_LEAVE_TYPE_MASTER,
    PPM_EMPLOYEE_LEAVE_DETAILS,ESS_LEAVE_REQ
WHERE
    PEMP_EMP_COMPANY_CODE=PLTM_COMP_CODE AND
    PEMP_EMP_CODE=PELD_CODE 
    and PELD_LEAVE_TYPE=PLTM_LEAVE_TYPE_CODE 
    AND PLTM_SHOWIN_LVBALANCE ='Y' AND  PEMP_EMP_CODE=ELR_EMP_ID(+) AND PLTM_LEAVE_TYPE_CODE=ELR_LEAVE_ID(+) AND ELR_REQ_STATUS_ID(+)=3     
    and PEMP_EMP_CODE='" + empCode + @"'
     GROUP BY  PLTM_LEAVE_TYPE_CODE,PELD_LEAVE_TYPE,PLTM_LEAVE_TYPE_DESC,PLTM_LEAVE_ARABIC_DESC ,PELD_LEAVE_AVAILED_DAYS,  PLTM_IS_LEAVE_IN_HOURS ,PELD_LEAVE_DAYS,PELD_LEAVE_PROCESSED_DATE,
     PEMP_EMP_CODE, PEMP_EMP_NAME,PELD_LEAVE_AFTER_DAYS,PLTM_SORT_ORDER ORDER BY PLTM_SORT_ORDER    
 ");

            var rowInfo = @"<tr><td colspan=3>No data found.</td></tr>";

            var empRow = "";
            if (dt != null && dt.Rows.Count > 0)
            {
                int II = 1;
                rowInfo = @"";

                empRow = @"<div class="""">
                                            <div class='col-md-6'  style='padding-left:0px;'><span style='font-weight: bold;'>" + ERP.Resources.Ess.EmpCode + @"</span>:" + dt.Rows[0]["PEMP_EMP_CODE"].ToString() + @"</div>
                                            <div class='col-md-6'> <span  style='font-weight: bold;' >" + ERP.Resources.Ess.EmployeeName + @"</span>:" + dt.Rows[0]["PEMP_EMP_NAME"].ToString() + @"</div>
                                        </div>";
                //PEMP_EMP_CODE, PEMP_EMP_NAME
                foreach (DataRow item in dt.Rows)
                {
                    string leaveHighLisght = "";
                    string foreColorStyle = "";  
                    if (item["PLTM_LEAVE_TYPE_CODE"].ToString() == LeaveId)
                    {
                        leaveHighLisght = @"style='background-color: #a1cbe8;'";
                        foreColorStyle = "style='bakcground-color: black;text-align:center;'";
                        
                    }

                    rowInfo += @"<tr " + leaveHighLisght + @"><td " + foreColorStyle + @"> " + II + @" </td>
                                                        <td " + foreColorStyle + @">" + item["PLTM_LEAVE_TYPE_DESC"].ToString() + @" </td>
                                                        <td " + foreColorStyle + @">" + item["WAITINGFORAPP"].ToString() + @" </td>
                                                        <td " + foreColorStyle + @">" + item["BALANCE_DAYS"].ToString() + @"</td>
                                                    </tr>
";
                    II++;
                }
            }


            var dataToReturn = @"<div class=""portlet light"">
                                    <div class="""">
                                        " + empRow + @"
                                        
                                    </div>
                                    <div class=""portlet-body"">
                                        <div class=""table-scrollable"">
                                            <table class=""table table-hover table-light"">
                                                <thead>
                                                    <tr>
                                                        <th> # </th>
                                                        <th> Leave Type </th>
                                                        <th> Waitifng for Approval </th>
                                                        <th> Balance </th>
                                                    </tr>
                                                </thead>
                                                <tbody>
                                                   " + rowInfo + @"
                                                </tbody>
                                            </table>
                                        </div>
                                    </div>
                                </div>";

            
            var JSONleavebalance = "";
            JSONleavebalance = JsonConvert.SerializeObject(dataToReturn);
            Response.Write(JSONleavebalance);
        }

        private void LEAVE_HISTORY(string empCode, string LeaveId)
        {
            DBAccess ObjDb = new DBAccess();

            var lang = "en-US";
            var query = @"

SELECT 
	 " + (lang == "en-US" ? @"
PLTM_LEAVE_TYPE_DESC, PEMP_EMP_NAME, EST_SERVICE_DESC, PBM_BRANCH_NAME, 
PEMP_EMP_DESIGNATION_DESC, PDPM_DEPARTMENT_DESC, PGDM_GRADE_DESC, CURR_STATUS_NAME," :
     @"
    nvl(PLTM_LEAVE_ARABIC_DESC, PLTM_LEAVE_TYPE_DESC) as PLTM_LEAVE_TYPE_DESC,
    nvl(PEMP_EMP_NAME_AR1,PEMP_EMP_NAME) as PEMP_EMP_NAME, nvl(EST_SERVICE_DESC_AR,EST_SERVICE_DESC) as EST_SERVICE_DESC, 
    nvl(PBM_BRANCH_NAME_ARABIC1, PBM_BRANCH_NAME) as PBM_BRANCH_NAME, nvl(PDPM_DEPT_AR1,PDPM_DEPARTMENT_DESC) as PDPM_DEPARTMENT_DESC, 
    nvl(PDSM_DESIGNATION_DESC_AR1,PEMP_EMP_DESIGNATION_DESC) as PEMP_EMP_DESIGNATION_DESC, nvl(PGDM_GRADE_DESC_AR1, PGDM_GRADE_DESC) as PGDM_GRADE_DESC, nvl(  ELS_STATUS_NAME_AR, CURR_STATUS_NAME) as CURR_STATUS_NAME, "
     )
+ @"
    PLTM_LEAVE_TYPE_CODE,
    ELRH_LEAVE_REQ_ID,
    EST_SERVICE_ID,
    PEMP_EMP_CODE,
    TO_CHAR (ELRH_REQUESTED_DT, 'dd-mm-yyyy')
    AS ELRH_REQUESTED_DT,
    TO_CHAR (FROM_DT, 'dd-mm-yyyy') AS FROM_DT,
    TO_CHAR (TO_DT, 'dd-mm-yyyy') AS TO_DT,
    ELRH_TOT_DAYS,
    SUBJ,
    LEAVE_BODY,
    ELRH_REQ_STATUS_ID,
    PLTM_LEAVE_TYPE_DESC, APPROVED_GROUP_NAME
FROM 
    ESS_LEAVE_REQ_DETAILS_ALL, ESS_REQ_APPROVED_GROUP
where 
   ELRH_SERVICE_TYPE in (1)
    AND PEMP_EMP_CODE ='" + empCode + @"'
    and PLTM_LEAVE_TYPE_CODE ='" + LeaveId + @"'
    and UNIQUE_ID_1(+)=  to_char(ELRH_LEAVE_REQ_ID)
    and UNIQUE_ID_2 (+)=to_char(ELRH_LEAVE_REQ_ID)


 ";
            DataTable dt = ObjDb.execute_query_retun_datatable(query);

                        AppCode.LogError.WriteToLogFile("leave_history", false, query);

            var rowInfo = @"<tr><td colspan=4>No data found.</td></tr>";

            var empRow = "";
            if (dt != null && dt.Rows.Count > 0)
            {
                int II = 1;
                rowInfo = @"";

                empRow = @"<div >
                                            <div class='col-md-6 '  style='padding-left:0px;'><span style='font-weight: bold;'>" + ERP.Resources.Ess.EmployeeCode + @"</span>: " + dt.Rows[0]["PEMP_EMP_CODE"].ToString() + @"</div>
                                            <div class='col-md-6'> <span  style='font-weight: bold;' >" + ERP.Resources.Ess.EmployeeName + ": </span> " + dt.Rows[0]["PEMP_EMP_NAME"].ToString() + @"</div>
                                        </div>";
                //PEMP_EMP_CODE, PEMP_EMP_NAME PLTM_LEAVE_TYPE_DESC
                foreach (DataRow item in dt.Rows)
                {
                    string leaveHighLisght = "";
                    string foreColorStyle = "";
                    if (item["PLTM_LEAVE_TYPE_CODE"].ToString() == LeaveId)
                    {
                        //leaveHighLisght = @"style='background-color: #a1cbe8;'";
                        //foreColorStyle = "style='bakcground-color: black;'";
                    }
                    //PLTM_LEAVE_TYPE_DESC
                    rowInfo += @"<tr " + leaveHighLisght + @"><td " + foreColorStyle + @"> " + II + @" </td>
                                                        <td " + foreColorStyle + @">" + item["PLTM_LEAVE_TYPE_DESC"].ToString() + @" </td>
                                                        <td " + foreColorStyle + @">" + item["FROM_DT"].ToString() + @"</td>
                                                        <td " + foreColorStyle + @">" + item["TO_DT"].ToString() + @"</td>
                                                        <td " + foreColorStyle + @">" + item["ELRH_TOT_DAYS"].ToString() + @"</td>
                                                        <td " + foreColorStyle + @">" + item["APPROVED_GROUP_NAME"].ToString() + @"</td>

                                                    </tr>
";
                    II++;
                }
            }


            var dataToReturn = @"<div class=""portlet light"">
                                    <div class="""">
                                        " + empRow + @"
                                        
                                    </div>
                                    <div class=""portlet-body"">
                                        <div class=""table-scrollable"">
                                            <table class=""table table-hover table-light"">
                                                <thead>
                                                    <tr>
                                                        <th> # </th>
                                                        <th>" + ERP.Resources.Ess.LeaveType + @"</th>
                                                        <th> " + ERP.Resources.Ess.From + @" </th>
                                                        <th> " + ERP.Resources.Ess.To + @" </th>
                                                        <th> " + ERP.Resources.Ess.TotalDays + @" </th>
                                                        <th> " + ERP.Resources.Ess.Status + @" </th>
                                                    </tr>
                                                </thead>
                                                <tbody>
                                                   " + rowInfo + @"
                                                </tbody>
                                            </table>
                                        </div>
                                    </div>
                                </div>";


            var JSONleaveresult = "";
            JSONleaveresult = JsonConvert.SerializeObject(dataToReturn);
            Response.Write(JSONleaveresult);
            
        }

        private void LEAVE_HISTORY_PLANNER(string empCode, string LeaveId)
        {
            DBAccess ObjDb = new DBAccess();

            var lang = "en-US";
            var query = @"

SELECT 
	 " + (lang == "en-US" ? @"
PLTM_LEAVE_TYPE_DESC, PEMP_EMP_NAME, EST_SERVICE_DESC, PBM_BRANCH_NAME, 
PEMP_EMP_DESIGNATION_DESC, PDPM_DEPARTMENT_DESC, PGDM_GRADE_DESC, CURR_STATUS_NAME," :
     @"
    nvl(PLTM_LEAVE_ARABIC_DESC, PLTM_LEAVE_TYPE_DESC) as PLTM_LEAVE_TYPE_DESC,
    nvl(PEMP_EMP_NAME_AR1,PEMP_EMP_NAME) as PEMP_EMP_NAME, nvl(EST_SERVICE_DESC_AR,EST_SERVICE_DESC) as EST_SERVICE_DESC, 
    nvl(PBM_BRANCH_NAME_ARABIC1, PBM_BRANCH_NAME) as PBM_BRANCH_NAME, nvl(PDPM_DEPT_AR1,PDPM_DEPARTMENT_DESC) as PDPM_DEPARTMENT_DESC, 
    nvl(PDSM_DESIGNATION_DESC_AR1,PEMP_EMP_DESIGNATION_DESC) as PEMP_EMP_DESIGNATION_DESC, nvl(PGDM_GRADE_DESC_AR1, PGDM_GRADE_DESC) as PGDM_GRADE_DESC, nvl(  ELS_STATUS_NAME_AR, CURR_STATUS_NAME) as CURR_STATUS_NAME, "
     )
+ @"
    PLTM_LEAVE_TYPE_CODE,
    ELRH_LEAVE_REQ_ID,
    EST_SERVICE_ID,
    PEMP_EMP_CODE,
    TO_CHAR (ELRH_REQUESTED_DT, 'dd-mm-yyyy')
    AS ELRH_REQUESTED_DT,
    TO_CHAR (FROM_DT, 'dd-mm-yyyy') AS FROM_DT,
    TO_CHAR (TO_DT, 'dd-mm-yyyy') AS TO_DT,
    ELRH_TOT_DAYS,
    SUBJ,
    LEAVE_BODY,
    ELRH_REQ_STATUS_ID,
    PLTM_LEAVE_TYPE_DESC, APPROVED_GROUP_NAME
FROM 
    ESS_LEAVE_REQ_DETAILS_ALL, ESS_REQ_APPROVED_GROUP
where 
   ELRH_SERVICE_TYPE in (37)
    AND PEMP_EMP_CODE ='" + empCode + @"'
    and PLTM_LEAVE_TYPE_CODE ='" + LeaveId + @"'
    and UNIQUE_ID_1(+)=  to_char(ELRH_LEAVE_REQ_ID)
    and UNIQUE_ID_2 (+)=to_char(ELRH_LEAVE_REQ_ID)


 ";
            DataTable dt = ObjDb.execute_query_retun_datatable(query);

            //            AppCode.LogError.WriteToLogFile("", false, query);

            var rowInfo = @"<tr><td colspan=4>No data found.</td></tr>";

            var empRow = "";
            if (dt != null && dt.Rows.Count > 0)
            {
                int II = 1;
                rowInfo = @"";

                empRow = @"<div >
                                            <div class='col-md-6 '  style='padding-left:0px;'><span style='font-weight: bold;'>" + ERP.Resources.Ess.EmployeeCode + @"</span>: " + dt.Rows[0]["PEMP_EMP_CODE"].ToString() + @"</div>
                                            <div class='col-md-6'> <span  style='font-weight: bold;' >" + ERP.Resources.Ess.EmployeeName + ": </span> " + dt.Rows[0]["PEMP_EMP_NAME"].ToString() + @"</div>
                                        </div>";
                //PEMP_EMP_CODE, PEMP_EMP_NAME PLTM_LEAVE_TYPE_DESC
                foreach (DataRow item in dt.Rows)
                {
                    string leaveHighLisght = "";
                    string foreColorStyle = "";
                    if (item["PLTM_LEAVE_TYPE_CODE"].ToString() == LeaveId)
                    {
                        //leaveHighLisght = @"style='background-color: #a1cbe8;'";
                        //foreColorStyle = "style='bakcground-color: black;'";
                    }
                    //PLTM_LEAVE_TYPE_DESC
                    rowInfo += @"<tr " + leaveHighLisght + @"><td " + foreColorStyle + @"> " + II + @" </td>
                                                        <td " + foreColorStyle + @">" + item["PLTM_LEAVE_TYPE_DESC"].ToString() + @" </td>
                                                        <td " + foreColorStyle + @">" + item["FROM_DT"].ToString() + @"</td>
                                                        <td " + foreColorStyle + @">" + item["TO_DT"].ToString() + @"</td>
                                                        <td " + foreColorStyle + @">" + item["ELRH_TOT_DAYS"].ToString() + @"</td>
                                                        <td " + foreColorStyle + @">" + item["APPROVED_GROUP_NAME"].ToString() + @"</td>

                                                    </tr>
";
                    II++;
                }
            }


            var dataToReturn = @"<div class=""portlet light"">
                                    <div class="""">
                                        " + empRow + @"
                                        
                                    </div>
                                    <div class=""portlet-body"">
                                        <div class=""table-scrollable"">
                                            <table class=""table table-hover table-light"">
                                                <thead>
                                                    <tr>
                                                        <th> # </th>
                                                        <th>" + ERP.Resources.Ess.LeaveType + @"</th>
                                                        <th> " + ERP.Resources.Ess.From + @" </th>
                                                        <th> " + ERP.Resources.Ess.To + @" </th>
                                                        <th> " + ERP.Resources.Ess.TotalDays + @" </th>
                                                        <th> " + ERP.Resources.Ess.Status + @" </th>
                                                    </tr>
                                                </thead>
                                                <tbody>
                                                   " + rowInfo + @"
                                                </tbody>
                                            </table>
                                        </div>
                                    </div>
                                </div>";


            var JSONleaveresult = "";
            JSONleaveresult = JsonConvert.SerializeObject(dataToReturn);
            Response.Write(JSONleaveresult);

        }
        private void LEAVECANCEL_HISTORY(string empCode, string LeaveId)
        {
            DBAccess ObjDb = new DBAccess();

            var lang = "en-US";
            var query = @"

SELECT 
	 " + (lang == "en-US" ? @"
PLTM_LEAVE_TYPE_DESC, PEMP_EMP_NAME, EST_SERVICE_DESC, PBM_BRANCH_NAME, 
PEMP_EMP_DESIGNATION_DESC, PDPM_DEPARTMENT_DESC, PGDM_GRADE_DESC, CURR_STATUS_NAME," :
     @"
    nvl(PLTM_LEAVE_ARABIC_DESC, PLTM_LEAVE_TYPE_DESC) as PLTM_LEAVE_TYPE_DESC,
    nvl(PEMP_EMP_NAME_AR1,PEMP_EMP_NAME) as PEMP_EMP_NAME, nvl(EST_SERVICE_DESC_AR,EST_SERVICE_DESC) as EST_SERVICE_DESC, 
    nvl(PBM_BRANCH_NAME_ARABIC1, PBM_BRANCH_NAME) as PBM_BRANCH_NAME, nvl(PDPM_DEPT_AR1,PDPM_DEPARTMENT_DESC) as PDPM_DEPARTMENT_DESC, 
    nvl(PDSM_DESIGNATION_DESC_AR1,PEMP_EMP_DESIGNATION_DESC) as PEMP_EMP_DESIGNATION_DESC, nvl(PGDM_GRADE_DESC_AR1, PGDM_GRADE_DESC) as PGDM_GRADE_DESC, nvl(  ELS_STATUS_NAME_AR, CURR_STATUS_NAME) as CURR_STATUS_NAME, "
     )
+ @"
    PLTM_LEAVE_TYPE_CODE,
    ELRH_LEAVE_REQ_ID,
    EST_SERVICE_ID,
    PEMP_EMP_CODE,
    TO_CHAR (ELRH_REQUESTED_DT, 'dd-mm-yyyy')
    AS ELRH_REQUESTED_DT,
    TO_CHAR (FROM_DT, 'dd-mm-yyyy') AS FROM_DT,
    TO_CHAR (TO_DT, 'dd-mm-yyyy') AS TO_DT,
    ELRH_TOT_DAYS,
    SUBJ,
    LEAVE_BODY,
    ELRH_REQ_STATUS_ID,
    PLTM_LEAVE_TYPE_DESC, APPROVED_GROUP_NAME
FROM 
    ESS_LEAVE_REQ_DETAILS_ALL, ESS_REQ_APPROVED_GROUP
where 
   ELRH_SERVICE_TYPE in (28)
    AND PEMP_EMP_CODE ='" + empCode + @"'
    and PLTM_LEAVE_TYPE_CODE ='" + LeaveId + @"'
    and UNIQUE_ID_1(+)=  to_char(ELRH_LEAVE_REQ_ID)
    and UNIQUE_ID_2 (+)=to_char(ELRH_LEAVE_REQ_ID)


 ";
            DataTable dt = ObjDb.execute_query_retun_datatable(query);

            //            AppCode.LogError.WriteToLogFile("", false, query);

            var rowInfo = @"<tr><td colspan=4>No data found.</td></tr>";

            var empRow = "";
            if (dt != null && dt.Rows.Count > 0)
            {
                int II = 1;
                rowInfo = @"";

                empRow = @"<div >
                                            <div class='col-md-6 '  style='padding-left:0px;'><span style='font-weight: bold;'>Employee Code</span>: " + dt.Rows[0]["PEMP_EMP_CODE"].ToString() + @"</div>
                                            <div class='col-md-6'> <span  style='font-weight: bold;' >Name: </span> " + dt.Rows[0]["PEMP_EMP_NAME"].ToString() + @"</div>
                                        </div>";
                //PEMP_EMP_CODE, PEMP_EMP_NAME PLTM_LEAVE_TYPE_DESC
                foreach (DataRow item in dt.Rows)
                {
                    string leaveHighLisght = "";
                    string foreColorStyle = "";
                    if (item["PLTM_LEAVE_TYPE_CODE"].ToString() == LeaveId)
                    {
                        //leaveHighLisght = @"style='background-color: #a1cbe8;'";
                        //foreColorStyle = "style='bakcground-color: black;'";
                    }
                    //PLTM_LEAVE_TYPE_DESC
                    rowInfo += @"<tr " + leaveHighLisght + @"><td " + foreColorStyle + @"> " + II + @" </td>
                                                        <td " + foreColorStyle + @">" + item["PLTM_LEAVE_TYPE_DESC"].ToString() + @" </td>
                                                        <td " + foreColorStyle + @">" + item["FROM_DT"].ToString() + @"</td>
                                                        <td " + foreColorStyle + @">" + item["TO_DT"].ToString() + @"</td>
                                                        <td " + foreColorStyle + @">" + item["ELRH_TOT_DAYS"].ToString() + @"</td>
                                                        <td " + foreColorStyle + @">" + item["APPROVED_GROUP_NAME"].ToString() + @"</td>

                                                    </tr>
";
                    II++;
                }
            }


            var dataToReturn = @"<div class=""portlet light"">
                                    <div class="""">
                                        " + empRow + @"
                                        
                                    </div>
                                    <div class=""portlet-body"">
                                        <div class=""table-scrollable"">
                                            <table class=""table table-hover table-light"">
                                                <thead>
                                                    <tr>
                                                        <th> # </th>
                                                        <th> Leave Type </th>
                                                        <th> From </th>
                                                        <th> To </th>
                                                        <th> Total Days </th>
                                                        <th> Status </th>
                                                    </tr>
                                                </thead>
                                                <tbody>
                                                   " + rowInfo + @"
                                                </tbody>
                                            </table>
                                        </div>
                                    </div>
                                </div>";

            ResponseWrite(dataToReturn);
        }
        private void ANNUALLEAVE_HISTORY(string empCode, string LeaveId)
        {
            DBAccess ObjDb = new DBAccess();

            var lang = "en-US";
            var query = @"

SELECT 
	 " + (lang == "en-US" ? @"
 PEMP_EMP_NAME, EST_SERVICE_DESC, PBM_BRANCH_NAME, 
PEMP_EMP_DESIGNATION_DESC, PDPM_DEPARTMENT_DESC, PGDM_GRADE_DESC, CURR_STATUS_NAME," :
     @"
    nvl(PLTM_LEAVE_ARABIC_DESC, PLTM_LEAVE_TYPE_DESC) as PLTM_LEAVE_TYPE_DESC,
    nvl(PEMP_EMP_NAME_AR1,PEMP_EMP_NAME) as PEMP_EMP_NAME, nvl(EST_SERVICE_DESC_AR,EST_SERVICE_DESC) as EST_SERVICE_DESC, 
    nvl(PBM_BRANCH_NAME_ARABIC1, PBM_BRANCH_NAME) as PBM_BRANCH_NAME, nvl(PDPM_DEPT_AR1,PDPM_DEPARTMENT_DESC) as PDPM_DEPARTMENT_DESC, 
    nvl(PDSM_DESIGNATION_DESC_AR1,PEMP_EMP_DESIGNATION_DESC) as PEMP_EMP_DESIGNATION_DESC, nvl(PGDM_GRADE_DESC_AR1, PGDM_GRADE_DESC) as PGDM_GRADE_DESC, nvl(  ELS_STATUS_NAME_AR, CURR_STATUS_NAME) as CURR_STATUS_NAME, "
     )
+ @"
    PLTM_LEAVE_TYPE_CODE,
    ELRH_LEAVE_REQ_ID,
    EST_SERVICE_ID,
    PEMP_EMP_CODE,
    TO_CHAR (ELRH_REQUESTED_DT, 'dd-mm-yyyy')
    AS ELRH_REQUESTED_DT,
    TO_CHAR (FROM_DT, 'dd-mm-yyyy') AS FROM_DT,
    TO_CHAR (TO_DT, 'dd-mm-yyyy') AS TO_DT,
    ELRH_TOT_DAYS,
    SUBJ,
    LEAVE_BODY,
    ELRH_REQ_STATUS_ID,
   'LEAVE RESUME' PLTM_LEAVE_TYPE_DESC, APPROVED_GROUP_NAME
FROM 
    ESS_LEAVE_REQ_DETAILS_ALL, ESS_REQ_APPROVED_GROUP
where 
   ELRH_SERVICE_TYPE in (26)
    AND PEMP_EMP_CODE ='" + empCode + @"'
    and PLTM_LEAVE_TYPE_CODE ='" + LeaveId + @"'
    and UNIQUE_ID_1(+)=  to_char(ELRH_LEAVE_REQ_ID)
    and UNIQUE_ID_2 (+)=to_char(ELRH_LEAVE_REQ_ID)


 ";
            DataTable dt = ObjDb.execute_query_retun_datatable(query);

            //            AppCode.LogError.WriteToLogFile("", false, query);

            var rowInfo = @"<tr><td colspan=4>No data found.</td></tr>";

            var empRow = "";
            if (dt != null && dt.Rows.Count > 0)
            {
                int II = 1;
                rowInfo = @"";

                empRow = @"<div >
                                            <div class='col-md-6 '  style='padding-left:0px;'><span style='font-weight: bold;'>Employee Code</span>: " + dt.Rows[0]["PEMP_EMP_CODE"].ToString() + @"</div>
                                            <div class='col-md-6'> <span  style='font-weight: bold;' >Name: </span> " + dt.Rows[0]["PEMP_EMP_NAME"].ToString() + @"</div>
                                        </div>";
                //PEMP_EMP_CODE, PEMP_EMP_NAME PLTM_LEAVE_TYPE_DESC
                foreach (DataRow item in dt.Rows)
                {
                    string leaveHighLisght = "";
                    string foreColorStyle = "";
                    if (item["PLTM_LEAVE_TYPE_CODE"].ToString() == LeaveId)
                    {
                        //leaveHighLisght = @"style='background-color: #a1cbe8;'";
                        //foreColorStyle = "style='bakcground-color: black;'";
                    }
                    //PLTM_LEAVE_TYPE_DESC 
                    rowInfo += @"<tr " + leaveHighLisght + @"><td " + foreColorStyle + @"> " + II + @" </td>
                                                        <td " + foreColorStyle + @">" + item["PLTM_LEAVE_TYPE_DESC"].ToString() + @" </td>
                                                        <td " + foreColorStyle + @">" + item["FROM_DT"].ToString() + @"</td>
                                                        <td " + foreColorStyle + @">" + item["TO_DT"].ToString() + @"</td>
                                                        <td " + foreColorStyle + @">" + item["ELRH_TOT_DAYS"].ToString() + @"</td>
                                                        <td " + foreColorStyle + @">" + item["APPROVED_GROUP_NAME"].ToString() + @"</td>

                                                    </tr>
";
                    II++;
                }
            }


            var dataToReturn = @"<div class=""portlet light"">
                                    <div class="""">
                                        " + empRow + @"
                                        
                                    </div>
                                    <div class=""portlet-body"">
                                        <div class=""table-scrollable"">
                                            <table class=""table table-hover table-light"">
                                                <thead>
                                                    <tr>
                                                        <th> # </th>
                                                        <th> Leave Type </th>
                                                        <th> From </th>
                                                        <th> To </th>
                                                        <th> Total Days </th>
                                                        <th> Status </th>
                                                    </tr>
                                                </thead>
                                                <tbody>
                                                   " + rowInfo + @"
                                                </tbody>
                                            </table>
                                        </div>
                                    </div>
                                </div>";

            ResponseWrite(dataToReturn);
        }
        private void PERMISSION_HISTORY(string empCode, string LeaveId)
        {
            DBAccess ObjDb = new DBAccess();

            var lang = "en-US";
            var query = @"

SELECT ELR_LEAVE_REQ_HEAD_ID,EMPID,ENAME,
    ELR_EMP_ID, ELR_LEAVE_ID, to_char(ELR_FROM_DT,'dd/mm/yyyy') ELR_FROM_DT, 
    to_char(ELR_TO_DT,'dd/mm/yyyy') ELR_TO_DT, ELR_TOT_DAYS, ELR_BODY, 
    ELR_REQ_STATUS_ID, ELR_TRAIN_PURPOSE,ELR_TRAIN_GOAL,
    EMPID, ENAME,PPT_PERMISSION_DESC,to_char(ELR_REQUESTED_DT,'dd/mm/yyyy')REQ_DATE,
    BRANCH_NAME, DEPT_NAME, DIV_NAME, DESIGNATION_DESC,

CASE WHEN ELR_REQ_STATUS_ID = 11  THEN 
            --'Waiting For ' || Replace(INITCAP(AEG_DESCRIPTION),'Waiting For','') 
            GET_LEAVE_STATUS (ELR_REQ_STATUS_ID)     
        ELSE  
            APPROVED_GROUP_NAME
        END   CURR_STATUS_NAME
,
CASE WHEN ELR_REQ_STATUS_ID = 11  THEN 
            --'Waiting For ' || Replace(INITCAP(AEG_DESCRIPTION),'Waiting For','') 
            GET_LEAVE_STATUS (ELR_REQ_STATUS_ID)     
        ELSE  
            APPROVED_GROUP_NAME
        END   APPROVED_GROUP_NAME
,
    APP_CNT
    
FROM 
    ESS_LEAVE_REQ,PPM_PERMISSION_TYPES,
    vu_emp_det,ESS_REQ_APPROVED_GROUP,
      ( select AWDD_WF_HEADER_ROW_ID WF_HEADER_ID, SUM(CASE WHEN wd2.AWDD_STATUS in ('A','R') THEN 1 ELSE 0 END) AS APP_CNT
  from AUTH_WF_DOC_DETAIL wd2  
                    GROUP BY AWDD_WF_HEADER_ROW_ID)
where
    empid=elr_emp_id
    and ELR_SERVICE_TYPE=3
     AND ELR_LEAVE_ID = PPT_PERMISSION_CODE
     AND UNIQUE_ID_1 =  TO_CHAR(ELR_LEAVE_REQ_HEAD_ID)
                    AND UNIQUE_ID_2 = TO_CHAR(ELR_LEAVE_REQ_HEAD_ID) 
    and WF_HEADER_ID(+) = AWH_ROW_ID1
    
 AND ELR_EMP_ID = '" + empCode + @"'  AND ELR_LEAVE_ID='" + LeaveId + @"'
  order by ELR_LEAVE_REQ_HEAD_ID desc";
            DataTable dt = ObjDb.execute_query_retun_datatable(query);

            //            AppCode.LogError.WriteToLogFile("", false, query);

            var rowInfo = @"<tr><td colspan=4>No data found.</td></tr>";

            var empRow = "";
            if (dt != null && dt.Rows.Count > 0)
            {
                int II = 1;
                rowInfo = @"";

                empRow = @"<div >
                                            <div class='col-md-6 '  style='padding-left:0px;'><span style='font-weight: bold;'>Employee Code</span>: " + dt.Rows[0]["EMPID"].ToString() + @"</div>
                                            <div class='col-md-6'> <span  style='font-weight: bold;' >Name: </span> " + dt.Rows[0]["ENAME"].ToString() + @"</div>
                                        </div>";
                //PEMP_EMP_CODE, PEMP_EMP_NAME PLTM_LEAVE_TYPE_DESC
                foreach (DataRow item in dt.Rows)
                {
                    string leaveHighLisght = "";
                    string foreColorStyle = "";
                
                    rowInfo += @"<tr " + leaveHighLisght + @"><td " + foreColorStyle + @"> " + II + @" </td>
                                                        <td " + foreColorStyle + @">" + item["PPT_PERMISSION_DESC"].ToString() + @" </td>
                                                        <td " + foreColorStyle + @">" + item["ELR_FROM_DT"].ToString() + @"</td>
                                                        <td " + foreColorStyle + @">" + item["ELR_TO_DT"].ToString() + @"</td>
                                                        <td " + foreColorStyle + @">" + item["ELR_TRAIN_PURPOSE"].ToString() + @"</td>
                                                        <td " + foreColorStyle + @">" + item["ELR_TRAIN_GOAL"].ToString() + @"</td>
                                                        <td " + foreColorStyle + @">" + item["APPROVED_GROUP_NAME"].ToString() + @"</td>

                                                    </tr>
";
                    II++;
                }
            }


            var dataToReturn = @"<div class=""portlet light"">
                                    <div class="""">
                                        " + empRow + @"
                                        
                                    </div>
                                    <div class=""portlet-body"">
                                        <div class=""table-scrollable"">
                                            <table class=""table table-hover table-light"">
                                                <thead>
                                                    <tr>
                                                        <th> # </th>
                                                        <th> Permission Type </th>
                                                        <th> From Date</th>
                                                        <th> To Date</th>
                                                        <th> From Time </th>
                                                        <th> To Time </th>
                                                        <th> Status </th>
                                                    </tr>
                                                </thead>
                                                <tbody>
                                                   " + rowInfo + @"
                                                </tbody>
                                            </table>
                                        </div>
                                    </div>
                                </div>";

            ResponseWrite(dataToReturn);
        }
        void CHECK_DATE()
        {
            var FromDate = Request.QueryString["FromDate"].ToUpper().ToArabicDateNull();
            var ToDate = Request.QueryString["ToDate"].ToUpper().ToArabicDateNull();

            if (FromDate.HasValue && ToDate.HasValue)
            {
                if (FromDate.Value > ToDate.Value)
                    WriteErrorMessage("Error: From date should be less than to date.");
            }

            WriteSuccessMessage("");
        }
        void CANCEL_REQUEST()
        {

            var req_id = Request.QueryString["ReqheadId"].ToUpper().ToLong();

            EssLeaveRequestDA da = new EssLeaveRequestDA(req_id);
            var otRefCode = "";
            try
            {
                if (da.leaveReqHeader.ELR_SERVICE_TYPE == ESSServices.ResignationRequest)
                {
                    otRefCode = da.leaveReqHeader.LeaveReqDetails[0].RefDocumentCode;
                }

            }
            catch { }
            if (da.DeleteLeaveRequest())
            {
                //otRefCode
                db.execute_query(@"
 delete from  ESS_OT_REQ_HEAD where EORH_OT_REQ_HEAER_ID ='" + otRefCode + @"' 
");


                WriteSuccessMessage("Request is deleted.");
            }
            else
            {
                ResponseWrite("Error: Unable to delete the Request");
            }





        }


        void REQ_DETAIL()
        {

            var req_id = Request.QueryString["ReqheadId"].ToUpper().ToLong();

            EssLeaveRequestDA da = new EssLeaveRequestDA(req_id);
            var lr = da.leaveReqHeader.LeaveReqDetails[0];
            Dictionary<string, string> list = new Dictionary<string, string>();
            var reqDetail = new
            {
                RequestType = lr.ELR_SERVICE_TYPE.ToString(),
                FromDate = lr.ELR_FROM_DT,
                ToDate = lr.ELR_TO_DT,
                Description = lr.ELR_BODY

            };
            var q = @"SELECT EMP_ID,
LEAVE_REQ_ID, ES_CODE, ES_DESC,EST_SERVICE_ID, 
   SERVICE_REQ_DESC,PLTM_LEAVE_TYPE_DESC,ATT_REASON_DESC1,
    LEAVE_BODY,AL_LETTER_NAME
FROM ESS_LEAVE_REQ_DETAILS_ALL where LEAVE_REQ_ID='" + req_id + @"'";

            //AppCode.LogError.WriteToLogFile("", false, q);
            var dt3 = db.execute_query_retun_datatable(q);
            if (dt3 == null || dt3.Rows.Count == 0)
            {
                return;
            }
            string reqType = "";
            string reqDesc = dt3.Rows[0]["LEAVE_BODY"].ToString();
            switch (lr.ELR_SERVICE_TYPE)
            {
                case ESSServices.Leave:
                    reqType = "Leave Request - " + dt3.Rows[0]["PLTM_LEAVE_TYPE_DESC"].ToString();
                    break;
                case ESSServices.LeaveCarryForward:
                    reqType = "Leave Carry Forward  ";
                    break;
                case ESSServices.Reimbursement:
                    break;
                case ESSServices.Permission:
                    reqType = "Permission Request";
                    break;
                case ESSServices.Grievance:
                    reqType = "Grievance";
                    break;
                case ESSServices.AttendanceAbsence:
                    reqType = "Attendance Request";
                    break;
                case ESSServices.RequestForTraining:
                    reqType = "Training Request";
                    break;
                case ESSServices.RequestForServices:
                    reqType = dt3.Rows[0]["ES_DESC"].ToString();
                    break;
                case ESSServices.TrainingFeedback:
                    reqType = "Training Feedback";
                    break;
                case ESSServices.RequestForPayslip:
                    reqType = "Request For Payslip";
                    break;
                case ESSServices.LoanRequest:
                    reqType = "Request For Loan";
                    break;
                case ESSServices.TravelRequest:
                    reqType = "Travel Request";
                    break;
                case ESSServices.TravelExpense:
                    reqType = "Travel Expense Request";
                    break;
                case ESSServices.ResignationRequest:
                    reqType = "Resignation Request";
                    break;
                case ESSServices.LetterRequest:
                    reqType = "Letter Request";
                    // GET LETTER TYPE
                    var letterType = dt3.Rows[0]["AL_LETTER_NAME"].ToString();
                    reqDesc = letterType;
                    break;
                case ESSServices.CompoffRequest:
                    reqType = "Compoff Request";
                    break;
                case ESSServices.RecruitmentRequest:
                    reqType = "Recruitment Request";
                    break;
                case ESSServices.GeneralRequest:
                    reqType = "General Request";
                    break;
                case ESSServices.BudegtRequest:
                    reqType = "Budget Request";
                    break;

                case ESSServices.AnnualLeaveResume:
                    reqType = "Leave Resume";
                    break;
                case ESSServices.Appointment:
                    reqType = "Entry Request";
                    reqDesc = objDB.execute_scalar(@"select VAD_VISITOR_NAME || '-' || VAD_VISITOR_COMPANY DETT from vmt_appointment_details , ess_leave_req where ELR_LEAVE_REQ_HEAD_ID='" + req_id + @"' and  ELR_REF_DOC_NO = to_char(VAD_APPOINTMENT_ID) ");
 
                    break;
                default:
                    reqType = lr.ELR_SERVICE_TYPE.ToString();
                    break;
            }
            var fname = Getfilinfo(dt3.Rows[0]["EST_SERVICE_ID"].ToString(), dt3.Rows[0]["EMP_ID"].ToString(), req_id.ToString());

            var JSONresult = "";
            if (fname != "")
            {
                // var file = fname.Substring(fname.LastIndexOf("/"), fname.Length);
                JSONresult = @"
                        <div class='row' >
                            <div class=' col-md-4'>
                                <div class='form-group'>
                                    <label>Request Type:</label>
                                </div>
                            </div>
                            <div class=' col-md-8'>
                                <div class='form-group'>
                                    <label>" + reqType + @"</label>
                                </div>
                            </div>

                        </div>
                        <div class='row' >
                            <div class=' col-md-4'>
                                <div class='form-group'>
                                    <label>Desciption:</label>
                                </div>
                            </div>
                            <div class=' col-md-8'>
                                <div class='form-group'>
                                    <label>" + reqDesc + @"</label>
                                </div>
                            </div>

                        </div>

                        <div class='row' >
                            <div class=' col-md-4'>
                                <div class='form-group'>
                                    <label>Ticket Number:</label>
                                </div>
                            </div>
                            <div class=' col-md-8'>
                                <div class='form-group'>
                                    <label>" + req_id + @"</label>
                                </div>
                            </div>
    
                        </div>
                    <div class='row' >
                            <div class=' col-md-4'>
                                <div class='form-group'>
                                    <label>Attachment:</label>
                                </div>
                            </div>
                            <div class=' col-md-8'>
                               <div class='form-group'>                                   
                                   <a download=" + fname + " href='" + fname + @"' target='_blank'><img src='" + ResolveUrl("~/images/blue.png") + @"'/></a>
                                </div>
                            </div>
    
                        </div>";
            }
            else
            {
                JSONresult = @"
                        <div class='row' >
                            <div class=' col-md-4'>
                                <div class='form-group'>
                                    <label>Request Type:</label>
                                </div>
                            </div>
                            <div class=' col-md-8'>
                                <div class='form-group'>
                                    <label>" + reqType + @"</label>
                                </div>
                            </div>

                        </div>
                        <div class='row' >
                            <div class=' col-md-4'>
                                <div class='form-group'>
                                    <label>Desciption:</label>
                                </div>
                            </div>
                            <div class=' col-md-8'>
                                <div class='form-group'>
                                    <label>" + reqDesc + @"</label>
                                </div>
                            </div>

                        </div>

                        <div class='row' >
                            <div class=' col-md-4'>
                                <div class='form-group'>
                                    <label>Ticket Number:</label>
                                </div>
                            </div>
                            <div class=' col-md-8'>
                                <div class='form-group'>
                                    <label>" + req_id + @"</label>
                                </div>
                            </div>

                        </div>
                 ";
            }
            ResponseWrite(JSONresult);

        }

        void AER_LOGIN()
        {
            var userName = Request.Form["txtUserName"];
            var txtPassword = Request.Form["txtPassword"];
            var ddlRole = Request.Form["ddlRole"];
            var response2 = "";
            var v = true; //ValidateADUser(userName, txtPassword);

            if (ValidateADUser(userName, txtPassword))
            {

                if (objDB.execute_scalar(@"SELECT TAP_PARAVALUE FROM TAS_ATT_PARAM  where      TAP_PARANAME ='IS_AD_AUTHENTICATION' ") == "Y")
                {
                    loggedInUserNAme = objDB.execute_scalar(@"
                        SELECT USER_NAME FROM PPM_EMPLOYEE_DETAILS, AMM_USER_DETAILS, PPM_EMP_ACTIVE_DIRECTORY
                            WHERE    
                            NVL(PEMP_EMP_ACTIVE,'Y') ='Y' and  NVL(USER_ACTIVE_YN,'Y') ='Y' and
                            PEMP_EMP_CODE =  USER_EMP_CODE 
                            AND UPPER( PEA_ADDISPLAYNAME  )=  UPPER(PEMP_EMP_AD_LOGIN)
                            AND UPPER(PEA_ADDISPLAYNAME)  ='" + userName.Trim().ToUpper() + @"'
                        ");
                }
                else
                {
                    loggedInUserNAme = userName;
                }



                if (v)
                {
                    if (ddlRole == "")
                    {
                        response2 = "Choose role";
                        var jsonObject2 = new JObject(new JProperty("data", response2));
                        Response.Write(jsonObject2.ToString());
                        return;
                    }//SELECT TAP_PARAVALUE FROM TAS_ATT_PARAM  where      TAP_PARANAME ='IS_WINDOWS_AUTH'
                    var roleBelongsToApexORERP = objDB.execute_scalar(@"Select ROLE_SYSTEM_ID from AMM_ROLE_DETAILS WHERE ROLE_ID='" +
                        ddlRole + @"' ");

                    // ResponseWrite(roleBelongsToApexORERP.ToString());

                    if (roleBelongsToApexORERP == "A")
                    {
                        bool apexStatus = true;
                        string urlToSend = "";
                        string errorMsg = "";

                        var userId3 = objDB.execute_scalar(@"

SELECT   
    USER_ID
FROM 
    AMM_USER_DETAILS
where 
    upper(USER_NAME) = UPPER('" + userName + @"')
");
                        // apex redirection
                        var apexURL = objDB.execute_scalar(@"SELECT ACLP_VALUE  FROM AMM_COMPANY_LEVEL_PARAMETERS where ACLP_PARAMETER_ID='APEX_INTEG'  ");
                        if (string.IsNullOrEmpty(apexURL))
                        {
                            LogError.WriteToLogFile("apex integration", true, "Unable to redirect to apex url as it is not defined");
                            apexStatus = false;
                            errorMsg = "Apex URL is not defined, please contact admin";
                        }
                        else
                        {
                            // generate sec token and redirect
                            var secToken = objDB.execute_scalar(@"
SELECT USER_SECURITY_PKG.GET_SECURITY_TAG_FN FROM DUAL

");
                            if (secToken != "")
                            {

                                objDB.execute_query(@"
Insert into AMM_APEX_DOTNET_INTEG_LOG
   (SECURITY_TAG,user_id, USER_ROLE_ID)
 Values
   ('" + secToken + @"', '" + userId3 + @"', '" + ddlRole + @"')
");
                                LogError.WriteToLogFile("apex integration is succecssful", true, secToken);
                                apexStatus = true;

                                //http://185.226.125.107:8090/ords/ws_cafm/r/management1012300/login?P9999_SECURITY_TAG=*SECURITY_TAG*&clear=myclearcache
                                urlToSend = apexURL = apexURL.Replace("*SECURITY_TAG*", secToken);
                                //Response.Redirect(apexURL);
                            }
                            else
                            {
                                apexStatus = false;
                                errorMsg = "Unable to redirect to SECtOKEN IS NOT GENERATED";
                                LogError.WriteToLogFile("apex integration", true, "Unable to redirect to SECtOKEN IS NOT GENERATED");

                            }
                            Response.Write(@"{""apexStatus"":""" + apexStatus.ToString().ToUpper() + @""", ""urlToSend"": """ + urlToSend + @""", ""errorMsg"": """ + errorMsg + @""", ""IsFailed"": true  }");
                            Response.End();
                            return;
                        }
                    }

                    var sessinResponse = CrateSession(loggedInUserNAme.Trim().ToUpper(), "01", ddlRole, false);
                    if (sessinResponse)
                    {
                        //Response.Redirect(@"~/Home.aspx");
                        response2 = ("SUCCESS");
                    }
                    else if (isDuplicateLogin)
                    {
                        response2 = ("DUPLICATE");

                    }
                    else
                    {
                        response2 = "sessinResponse: " + sessinResponse + "so elese part";
                    }
                    //else if (ddlCompany.Items.Count == 0 || ddlRole.Items.Count == 0)
                    //ks.showAlert(@"alert('Unable to create session.');", this);
                    //  ks.showAlert(@"alert('Please choose the Company and Role.');", this);
                }
                else
                {
                    response2 = ("User name/ password is not matching");

                }
            }
			else
			{
                response2 = ("Invalid User name/ password");
            }
            var jsonObject = new JObject(new JProperty("data", response2));
            Response.Write(jsonObject.ToString());

        }

        void AER_LOGIN2()
        {
            var userName = Request.Form["ontcUserName"].ToUpper();
            var txtPassword = Request.Form["ontcPassword"];
            var ddlRole = Request.Form["ontcROlesDDL"];
            var response1 = "";
            if (ValidateADUser(userName, txtPassword))
            {
                if (ddlRole == "")
                {
                    response1 = ("Choose role");
                }
                else if (CrateSession(loggedInUserNAme.Trim(), "01", ddlRole, false))
                {
                    //Response.Redirect(@"~/Home.aspx");
                    response1 = ("SUCCESS");
                }
                else if (isDuplicateLogin)
                {
                    response1 = ("DUPLICATE");

                }

            }
            else
            {
                response1 = ("User name/ password is not matching");

            }

            Response.Write(@"{ ""data"" : """ + response1 + @"""}");
        }
        bool isDuplicateLogin = false;
        bool CrateSession(string ParUserName, string ParCompCode, string ParRoleId, bool IsRememberMeChecked)
        {
            bool IsSessionCreated = false;
            isDuplicateLogin = false;
            UserDatail userDetail = new UserDatail();
            Session["MENU"] = null;
            if (ParRoleId != null)
            {
                string strrole = "SELECT ROLE_NAME FROM AMM_ROLE_DETAILS WHERE ROLE_ID='" + ParRoleId.ToInt() + "'";

                userDetail.Role_Name = objDB.execute_scalar(strrole);
            }


            string str_select = @"
SELECT  PEMP_EMP_NAME,
    USER_ID, USER_SHORT_NAME, nvl(USER_EMP_CODE,'wits') USER_EMP_CODE, nvl(PEMP_EMP_CODE,'wits') PEMP_EMP_CODE,
    nvl(PEMP_EMP_DEPTARTMENT_CODE,'wits-dep') PEMP_EMP_DEPTARTMENT_CODE, PDPM_DEPARTMENT_DESC , 
    PEMP_EMP_DESIGNATION_CODE , PDSM_DESIGNATION_DESC,
    nvl(PEMP_EMP_BRANCH_CODE,'wits_br') PEMP_EMP_BRANCH_CODE,
    PBM_BRANCH_CODE, PBM_BRANCH_NAME,PEMP_EMP_NAME_AR,
    nvl(PEMP_IS_AMINISTRATOR,'N') as PEMP_IS_AMINISTRATOR,
    nvl(PEMP_HR_LV_APPR,'0') as PEMP_HR_LV_APPR, 
    nvl(PEMP_IS_BRANCH_ADMIN,'N') as PEMP_IS_BRANCH_ADMIN , PEMP_REPORTING_TO ,nvl(PEMP_BROWSE_EMPLOYEES,'N')PEMP_BROWSE_EMPLOYEES, nvl(PEMP_IS_DEPARTMENT_HEAD,'N') as PEMP_IS_DEPARTMENT_HEAD
FROM 
    AMM_USER_DETAILS , PPM_EMPLOYEE_DETAILS , PPM_BRANCH_MASTER , PPM_DEPARTMENT_MASTER , PPM_DESIGNATION_MASTER
where 
    upper(USER_NAME) = UPPER('" + ParUserName + @"')
    AND PEMP_EMP_CODE (+) = USER_EMP_CODE
    AND USER_DEFAULT_LOCATION = PBM_BRANCH_CODE(+)
    AND PEMP_EMP_DEPTARTMENT_CODE = PDPM_DEPARTMENT_CODE(+)
    AND PEMP_EMP_DESIGNATION_CODE = PDSM_DESIGNATION_CODE(+)
    and  AUD_COMP_CODE = PBM_COMP_CODE (+)
    
";
            ERP1.LogError.WriteToLogFile("", false, str_select);

            string str_select_func_curr_code = @"
SELECT  
    ACD_FUNCTIONAL_CURR_CODE,MCUM_DEC_PLACE,MCUM_SUB_UNIT 
FROM 
    AMM_COMPANY_DETAILS,MMM_CURRENCY_MASTER 
WHERE UPPER(ACD_COMP_CODE) = '" + ParCompCode + "' AND UPPER(MCUM_CURR_CODE) = UPPER(ACD_FUNCTIONAL_CURR_CODE) ";


            DataTable dt_result = objDB.execute_query_retun_datatable(str_select);
            DataTable dt_func_curr = objDB.execute_query_retun_datatable(str_select_func_curr_code);
            DataTable dtCompany = objDB.execute_query_retun_datatable(@"SELECT ACD_COMP_CODE, ACD_COMP_NAME ,ACD_COUNTRY_CODE , ACM_COUNTRY_NAME FROM AMM_COMPANY_DETAILS, AMM_COUNTRY_MASTER WHERE ACD_COUNTRY_CODE = ACM_COUNTRY_CODE AND ACD_COMP_CODE = '" + ParCompCode + @"'  ");

            try
            {
                if (dt_result != null && dt_result.Rows.Count > 0 && dtCompany != null && dtCompany.Rows.Count > 0)
                {

                    userDetail.IsAdministrator = dt_result.Rows[0]["PEMP_IS_AMINISTRATOR"].ToString().Equals("Y");

                    userDetail.IsAdministrator = dt_result.Rows[0]["PEMP_IS_AMINISTRATOR"].ToString().Equals("Y");
                    userDetail.IsHRLeaveApprover = dt_result.Rows[0]["PEMP_HR_LV_APPR"].ToString().Equals("1");
                    userDetail.IsBranchAdministrator = dt_result.Rows[0]["PEMP_IS_BRANCH_ADMIN"].ToString().Equals("Y");
                    userDetail.CanBrowseAllEmployees = dt_result.Rows[0]["PEMP_BROWSE_EMPLOYEES"].ToString().Equals("Y");
                    userDetail.IsDepartmentHead = dt_result.Rows[0]["PEMP_IS_DEPARTMENT_HEAD"].ToString().Equals("Y");

                    userDetail.Login_Company_Code = dtCompany.Rows[0]["ACD_COMP_CODE"].ToString();
                    userDetail.Login_Company_Name = dtCompany.Rows[0]["ACD_COMP_NAME"].ToString();
                    // userDetail.Role_Name = ddlRole.SelectedItem.Text.ToString();
                    userDetail.CompCountryCode = dtCompany.Rows[0]["ACD_COUNTRY_CODE"].ToString();
                    userDetail.CompCountryName = dtCompany.Rows[0]["ACM_COUNTRY_NAME"].ToString();

                    userDetail.User_Name = ParUserName;
                    userDetail.User_Id = dt_result.Rows[0]["USER_ID"].ToString().ToInt();
                    userDetail.User_Display_Name = dt_result.Rows[0]["USER_SHORT_NAME"].ToString();
                    userDetail.Emp_Name_Arabic = dt_result.Rows[0]["PEMP_EMP_NAME_AR"].ToString();
                    userDetail.Emp_Code = dt_result.Rows[0]["USER_EMP_CODE"].ToString();
                    userDetail.Emp_Name = dt_result.Rows[0]["PEMP_EMP_NAME"].ToString();
                    userDetail.Role_Id = ParRoleId.ToInt();
                    userDetail.DepartmentCode = dt_result.Rows[0]["PEMP_EMP_DEPTARTMENT_CODE"].ToString();
                    userDetail.DepartmentName = dt_result.Rows[0]["PDPM_DEPARTMENT_DESC"].ToString();
                    userDetail.BranchCode = dt_result.Rows[0]["PEMP_EMP_BRANCH_CODE"].ToString();
                    userDetail.DefaultBranchCode = dt_result.Rows[0]["PEMP_EMP_BRANCH_CODE"].ToString();
                    userDetail.DefaultBranchDesc = dt_result.Rows[0]["PBM_BRANCH_NAME"].ToString();
                    userDetail.ManagerEmpCode = dt_result.Rows[0]["PEMP_REPORTING_TO"].ToString();
                    userDetail.ManagerEmpName = objDB.execute_scalar("SELECT PEMP_EMP_NAME FROM PPM_EMPLOYEE_DETAILS WHERE PEMP_EMP_CODE = '" + dt_result.Rows[0]["PEMP_REPORTING_TO"].ToString() + "'");
                    userDetail.DesignationCode = dt_result.Rows[0]["PEMP_EMP_DESIGNATION_CODE"].ToString(); ;
                    userDetail.DesignationName = dt_result.Rows[0]["PDSM_DESIGNATION_DESC"].ToString(); ;
                    //    LogError.WriteToLogFile("",false, @"SELECT ROLE_ID, MODULE_ID, MODULE_CODE, MODULE_NAME, MODULE_OTH_NAME FROM AMM_ROLE_DETAILS,AMM_MODULES_MASTER 
                    //WHERE ROLE_MODULE_ID   = MODULE_ID AND ROLE_ID= '" + userDetail.Role_Id + "'     ");
                    DataTable dt_Module = objDB.execute_query_retun_datatable(@"SELECT ROLE_ID, MODULE_ID, MODULE_CODE, MODULE_NAME, MODULE_OTH_NAME FROM AMM_ROLE_DETAILS,AMM_MODULES_MASTER 
                WHERE ROLE_MODULE_ID   = MODULE_ID AND ROLE_ID= '" + userDetail.Role_Id + "'     ");//and ROLE_COMP_CODE='" + ParCompCode + @"'
                    if (dt_Module != null && dt_Module.Rows.Count > 0)
                    {
                        userDetail.ModuleId = int.Parse(dt_Module.Rows[0]["MODULE_ID"].ToString());
                        userDetail.ModuleCode = dt_Module.Rows[0]["MODULE_CODE"].ToString();
                        userDetail.ModuleName = dt_Module.Rows[0]["MODULE_NAME"].ToString();
                        userDetail.ModuleNameAR = dt_Module.Rows[0]["MODULE_OTH_NAME"].ToString();
                    }
                    userDetail.Role_Logo = objDB.execute_scalar(@"SELECT MCMD_ENTITY_DESC FROM AMM_ROLE_DETAILS, ACCMD_GET_COMMON_MASTER WHERE 
                ROLE_ID = '" + userDetail.Role_Id + @"' 
                and MCMH_ENTITY_GROUP = 'APP_LOGO'
                and MCMD_ENTITY_CODE = ROLE_LOGO_PATH");

                    //ROLE_READ_ONLY

                    Session["ROLE_READ_ONLY"] = objDB.execute_scalar(@"SELECT ROLE_READ_ONLY FROM AMM_ROLE_DETAILS  WHERE 
                ROLE_ID = '" + userDetail.Role_Id + @"' ");

                    int tasId = 0;
                    int.TryParse(objDB.execute_scalar(@"Select F_GET_TAS_ID('01','" + userDetail.Emp_Code + "') from dual"), out tasId);
                    userDetail.TASID = tasId;
                    // L_USER,  L_COMP_CODE ,  L_ROLE_ID
                    if (IsRememberMeChecked)
                    {
                        Response.Cookies.Add(new HttpCookie("L_USER") { Expires = DateTime.Now.AddDays(10), Value = ParUserName });
                        Response.Cookies.Add(new HttpCookie("L_COMP_CODE") { Expires = DateTime.Now.AddDays(10), Value = ParCompCode });
                        Response.Cookies.Add(new HttpCookie("L_ROLE_ID") { Expires = DateTime.Now.AddDays(10), Value = ParRoleId });
                    }



                    if (dt_func_curr != null && dt_func_curr.Rows.Count > 0)
                    {
                        userDetail.base_curr_code = dt_func_curr.Rows[0]["ACD_FUNCTIONAL_CURR_CODE"].ToString();
                        userDetail.base_curr_decimal_places = dt_func_curr.Rows[0]["MCUM_DEC_PLACE"].ToString();
                        userDetail.CurrencySubUnit = dt_func_curr.Rows[0]["MCUM_SUB_UNIT"].ToString();
                        //Response.Redirect("home.aspx");
                    }
                    if (Session.Contents["Culture"] == null)
                        userDetail.UserCulture = "";
                    else
                        userDetail.UserCulture = Session.Contents["Culture"].ToString();

                    Session["userDetail"] = userDetail;
                    ERP.ERPSiteMapProvider erpSiteMapProvider = new ERP.ERPSiteMapProvider();
                    //erpSiteMapProvider.IsInitialized = false;
                    //erpSiteMapProvider.ClearSiteMap();
                    ERP.ERPSiteMapManager.Clear();
                    SessionManager.CreateERPSession(userDetail.User_Name, userDetail.Role_Id, userDetail.Login_Company_Code);
                    IsSessionCreated = true;
                    LoggedUser loggedUser = new LoggedUser
                    {
                        UserId = userDetail.User_Id,
                        SessionId = Session.SessionID,
                        IPAddress = Request.UserHostAddress,
                        BrowserName = Request.UserAgent + "---" + Request.Browser.Browser + "Major - " + Request.Browser.MajorVersion + " Minor - " + Request.Browser.MinorVersion,
                        IsLoggedOn = true,
                        Date = DateTime.Now
                    };
                    if (ConfigurationManager.AppSettings["SessionCheck"] == "TRUE")
                    {
                        if (UserSignOnManager.GetInstatnce.IsUserExitsInOtherSession(loggedUser))
                        {
                            //IsSessionCreated = true;
                            //if (ConfigurationManager.AppSettings["SessionCheck"]=="TRUE")
                            {
                                IsSessionCreated = false;
                                isDuplicateLogin = true;
                            }
                        }
                        else
                        {
                            isDuplicateLogin = false;
                            UserSignOnManager.GetInstatnce.AddUser(loggedUser);
                        }
                    }
                    else
                    {
                        IsSessionCreated = true;
                        UserSignOnManager.GetInstatnce.AddUser(loggedUser);
                    }

                }
                else
                {
                    ERP1.LogError.WriteToLogFile("errorr ", false, "error in create sessionerror in create sessionerror in create session");

                }

            }
            catch (Exception e1)
            {
                AppCode.LogError.WriteToLogFile("CrateSession(string ParUserName, string ParCompCode, string ParRoleId, bool IsRememberMeChecked)", "", e1.ToString());
            }
            ERP.Log.LogSessionCount.UserLoggedIn();

            return IsSessionCreated;
        }
        void USER_ROLES_ONTC()
        {
            // LogError.WriteToLogFile("", false, "USER_ROLES_ONTC" );

            var userName = Request.Form["ontcUserName"].ToUpper();
            var txtPassword = Request.Form["ontcPassword"];
            var ontcCompanyDDL = Request.Form["ontcCompanyDDL"];
            dbaccess dbNew = new dbaccess(ontcCompanyDDL);                //ontcUserName ontcPassword ontcCompanyDDL ontcROlesDDL
            if (!ValidateADUserONTC(userName, txtPassword, ontcCompanyDDL))
            {
                if (LastError != "")
                {
                    WriteErrorMessage(LastError);
                    return;
                }
                WriteErrorMessage("User not found.");
                return;
            }

            string par_command = @"SELECT   URL.URL_ROLE_ID, ARD.ROLE_NAME
          FROM AMM_USER_ROLE_LNK_DETAILS URL, AMM_ROLE_DETAILS ARD, AMM_USER_DETAILS AUD 
          WHERE URL.URL_ROLE_ID = ARD.ROLE_ID 
        AND URL.URL_USER_ID = AUD.USER_ID 
    
         AND UPPER(AUD.USER_NAME) ='" + GetUserName(userName) + @"'";
            LogError.WriteToLogFile("",false, par_command);
            var dt1 = dbNew.execute_query_retun_datatable(par_command);

            //ontcCompanyDDL
            var JSONresult = JsonConvert.SerializeObject(dt1);
            Response.Write(JSONresult);


        }
        bool ValidateADUserONTC(string userName, string password, string companyName

              )
        {

            //dbaccess dbNew = new dbaccess();
            dbaccess dbNew = new dbaccess(companyName);                //ontcUserName ontcPassword ontcCompanyDDL ontcROlesDDL

            loggedInUserNAme = userName;
            if (dbNew.execute_scalar(@"SELECT TAP_PARAVALUE FROM TAS_ATT_PARAM  where      TAP_PARANAME ='IS_AD_AUTHENTICATION' ") == "Y")
            {
                ADAuthentication aDAuthentication = new ADAuthentication();
                if (aDAuthentication.IsUserExistInAD(userName.Trim(), password))
                {

                    string userId = objDB.execute_scalar(@"SELECT USER_NAME FROM PPM_EMPLOYEE_DETAILS, AMM_USER_DETAILS WHERE    NVL(PEMP_EMP_ACTIVE,'Y') ='Y' and  NVL(USER_ACTIVE_YN,'Y') ='Y' and
  PEMP_EMP_CODE =  USER_EMP_CODE and UPPER(PEMP_EMP_AD_LOGIN) ='" + userName.Trim().ToUpper() + @"' ");
                    if (!string.IsNullOrEmpty(userId))
                    {
                        loggedInUserNAme = userId;

                        return true;
                    }
                    //                    ks.ErrorMessage("The user id is not found in dtabase. Please contact the admin.", this);
                    return false;
                }
                else
                {
                    //ks.ErrorMessage("The user not found/ password wrong. The active directory authentication failed", this);
                    return false;
                }
            }
            else
            {
                var passwordLockEnabled = dbNew.execute_scalar("SELECT  ACLP_VALUE  FROM  AMM_COMPANY_LEVEL_PARAMETERS where ACLP_PARAMETER_ID ='PASS_LOCK'   ").ToInt();


                string str_chk_user_pass_query = " SELECT USER_PASSWORD , USER_TEMPORARY_YN FROM AMM_USER_DETAILS,PPM_EMPLOYEE_DETAILS WHERE  NVL(PEMP_EMP_ACTIVE,'Y') ='Y' and PEMP_EMP_CODE =  USER_EMP_CODE and upper(USER_NAME) = '" + userName.ToString().Trim().ToUpper().Replace("'", "''") + "' ";
                //  str_chk_user_pass_query += " AND USER_PASSWORD ='" + password.ToString().Trim().Replace("'", "''") + "' ";
                var dt1 = dbNew.execute_query_retun_datatable(str_chk_user_pass_query);
                var passwordDB = dt1.Rows[0]["USER_PASSWORD"].ToString();
                var passwordLockedYN = dt1.Rows[0]["USER_TEMPORARY_YN"].ToString();
                if (passwordLockedYN == "Y")
                {

                    LastError = "User is locked, kindly contact IT.";
                    return false;
                }
                LogError.WriteToLogFile("", false, "str_chk_user_pass_query" + str_chk_user_pass_query);
                if (passwordDB == password.ToString().Trim())
                {
                    LogError.WriteToLogFile("", false, "return false");
                    return true;
                }
                else
                {
                    if (passwordLockEnabled <= 0)
                    {
                        return false;
                    }
                    LastError = "";
                    LogError.WriteToLogFile("", false, "return true");
                    var passwordAttemptCnt = dbNew.execute_scalar("SELECT NVL(USER_PASSWORD_ATTEMPT_CNT,0) FROM AMM_USER_DETAILS WHERE  UPPER(USER_NAME) = '" + userName.ToString().Trim().ToUpper().Replace("'", "''") + "'  ").ToInt();
                    if (passwordAttemptCnt > passwordLockEnabled)
                    {
                        dbNew.execute_scalar("UPDATE   AMM_USER_DETAILS SET  USER_TEMPORARY_YN ='Y', USER_PASSWORD_ATTEMPT_DATE = SYSDATE WHERE  UPPER(USER_NAME) = '" + userName.ToString().Trim().ToUpper().Replace("'", "''") + @"' ");
                        LastError = "User locked";
                    }
                    else
                    {
                        dbNew.execute_scalar("UPDATE   AMM_USER_DETAILS SET  USER_PASSWORD_ATTEMPT_CNT = nvl(USER_PASSWORD_ATTEMPT_CNT,0) + 1, USER_PASSWORD_ATTEMPT_DATE = SYSDATE WHERE  UPPER(USER_NAME) = '" + userName.ToString().Trim().ToUpper().Replace("'", "''") + @"' ");

                    }
                    return false;
                }
            }
        }
        string LastError = "";
        void USER_ROLES()
        {
            var userName = Request.Form["txtUserName"].ToUpper();
            var txtPassword = Request.Form["txtPassword"];
            if (!ValidateADUser(userName, txtPassword))
            {
                WriteErrorMessage("");
                return;
            }
            var WHERECOND = "";
            if (ConfigurationManager.AppSettings["INSTALLED_COMPANY"] == "MTI")
            {
                var str_role = objDB.execute_scalar(@"SELECT MODULE_ID FROM AMM_MODULES_MASTER WHERE MODULE_CODE='" + ConfigurationManager.AppSettings["INSTALLED_COMPANY"] + @"'");
                WHERECOND += " AND ROLE_MODULE_ID ='" + (string.IsNullOrEmpty(str_role) ? ConfigurationManager.AppSettings["MTI_MODULE_ID"]:str_role  ) +"' ";
            }
            var query1 = @"SELECT   URL.URL_ROLE_ID, ARD.ROLE_NAME
          FROM AMM_USER_ROLE_LNK_DETAILS URL, AMM_ROLE_DETAILS ARD, AMM_USER_DETAILS AUD 
          WHERE URL.URL_ROLE_ID = ARD.ROLE_ID 
        AND URL.URL_USER_ID = AUD.USER_ID 
    
         AND UPPER(AUD.USER_NAME) ='" + GetUserName(userName) + @"' " + WHERECOND;
                var dt1 = db.execute_query_retun_datatable(query1);
            //LogError.WriteToLogFile("query1", false, query1);

            var JSONresult = JsonConvert.SerializeObject(dt1);
            Response.Write(JSONresult);


        }
        dbaccess objDB = new dbaccess();
        string loggedInUserNAme = "";
        bool ValidateADUser(string userName, string password)
        {

            loggedInUserNAme = userName;

            string str_chk_user_pass_query = " SELECT COUNT(*) FROM AMM_USER_DETAILS,PPM_EMPLOYEE_DETAILS WHERE  NVL(PEMP_EMP_ACTIVE,'Y') ='Y' and PEMP_EMP_CODE =  USER_EMP_CODE and upper(USER_NAME) = '" + userName.ToString().Trim().ToUpper().Replace("'", "''") + "' ";
            str_chk_user_pass_query += " AND USER_PASSWORD ='" + password.ToString().Trim().Replace("'", "''") + "' ";

            if (int.Parse((objDB.record_found(str_chk_user_pass_query).ToString())) <= 0)
            {
                return false;
            }
            else
            {
                bool valid = true;

                if (objDB.execute_scalar(@"SELECT TAP_PARAVALUE FROM TAS_ATT_PARAM  where      TAP_PARANAME ='IS_AD_AUTHENTICATION' ") == "Y")
                {
                    ADAuthentication aDAuthentication = new ADAuthentication();
                    var aduserToSearch = objDB.execute_scalar(@"
                        SELECT PEAD_ADLOGINNAME FROM  PPM_EMP_ACTIVE_DIRECTORY
                            WHERE    
                             UPPER(PEA_ADDISPLAYNAME)  ='" + userName.Trim().ToUpper() + @"'
                        ");
                                            if (aDAuthentication.AuthenticateActiveDirectoryAccount(aduserToSearch.Trim(), password))
                                            {

                                                string userId = objDB.execute_scalar(@"
                        SELECT USER_NAME FROM PPM_EMPLOYEE_DETAILS, AMM_USER_DETAILS, PPM_EMP_ACTIVE_DIRECTORY
                            WHERE    
                            NVL(PEMP_EMP_ACTIVE,'Y') ='Y' and  NVL(USER_ACTIVE_YN,'Y') ='Y' and
                            PEMP_EMP_CODE =  USER_EMP_CODE 
                            AND UPPER( PEA_ADDISPLAYNAME  )=  UPPER(PEMP_EMP_AD_LOGIN)
                            AND UPPER(PEA_ADDISPLAYNAME)  ='" + userName.Trim().ToUpper() + @"'
                        ");
                        //and UPPER() ='" + userName.Trim().ToUpper() + @"'
                        if (!string.IsNullOrEmpty(userId))
                        {
                            loggedInUserNAme = userId;

                            valid= true;
                        }
                        //                    ks.ErrorMessage("The user id is not found in dtabase. Please contact the admin.", this);
                        valid= false;
                    }
                    else
                    {
                        //ks.ErrorMessage("The user not found/ password wrong. The active directory authentication failed", this);
                        valid= false;
                    }
                }
                return valid;
                //else
                //{

                //    string str_chk_user_pass_query = " SELECT COUNT(*) FROM AMM_USER_DETAILS,PPM_EMPLOYEE_DETAILS WHERE  NVL(PEMP_EMP_ACTIVE,'Y') ='Y' and PEMP_EMP_CODE =  USER_EMP_CODE and upper(USER_NAME) = '" + userName.ToString().Trim().ToUpper().Replace("'", "''") + "' ";
                //    str_chk_user_pass_query += " AND USER_PASSWORD ='" + password.ToString().Trim().Replace("'", "''") + "' ";

                //    if (int.Parse((objDB.record_found(str_chk_user_pass_query).ToString())) <= 0)
                //    {
                //        return false;
                //    }
                //    else
                //    {
                //        return true;
                //    }
                //}
            }
        }

        string GetUserName(string userName)
        {

            if (objDB.execute_scalar(@"SELECT TAP_PARAVALUE FROM TAS_ATT_PARAM  where      TAP_PARANAME ='IS_AD_AUTHENTICATION' ") == "Y")
            {
                ADAuthentication aDAuthentication = new ADAuthentication();
                string userId = objDB.execute_scalar(@"SELECT USER_NAME FROM PPM_EMPLOYEE_DETAILS, AMM_USER_DETAILS WHERE 
  PEMP_EMP_CODE =  USER_EMP_CODE and UPPER(PEMP_EMP_AD_LOGIN) ='" + userName.Trim().ToUpper() + @"' ");
                return userId;
            }
            else
            {

                return userName;
            }
        }

        void FILE_DELETE()
        {

            var filePathToDelete = Request.Form["EduFilePathToDelete"];

            //AppCode.LogError.WriteToLogFile("", false, filePathToDelete);


            var fileInfo = new FileInfo(MapPath(filePathToDelete));

            fileInfo.Delete();

            WriteSuccessMessage("File delete successfully.");


        }

        void TRAININGFILE_DELETE()
        {

            var filePathToDelete = Request.Form["TrainFilePathToDelete"];

            //AppCode.LogError.WriteToLogFile("", false, filePathToDelete);


            var fileInfo = new FileInfo(MapPath(filePathToDelete));

            fileInfo.Delete();

            WriteSuccessMessage("File delete successfully.");


        }

        void GET_QUAL_DOCUMENTS()
        {


            var srlNo = Request.QueryString["SrlNo"];
            var empCode = Request.QueryString["empCode"];


            var dt = new DataTable();

            dt.Columns.Add("SrlNo");
            dt.Columns.Add("FileName");
            dt.Columns.Add("RelativePath");
            dt.Columns.Add("DeletePath");
			if (Directory.Exists(Server.MapPath("~/HR/Docs/Education/" + empCode + "/" + srlNo)))
			{


            var files = Directory.GetFiles(Server.MapPath("~/HR/Docs/Education/" + empCode + "/" + srlNo));

            for (int i = 0; i < files.Length; i++)
            {

                var fileInf = new FileInfo(files[i]);

                var row = dt.NewRow();

                row["SrlNo"] = i + 1;
                row["FileName"] = fileInf.Name;
                row["RelativePath"] = ResolveUrl("~/HR/Docs/Education/" + empCode + "/" + srlNo) + "/" + fileInf.Name;
                row["DeletePath"] = ("~/HR/Docs/Education/" + empCode + "/" + srlNo) + "/" + fileInf.Name;


                dt.Rows.Add(row);


            }

            }

            var JSONresult = JsonConvert.SerializeObject(dt);
            Response.Write(JSONresult);
            //return JSONresult;
        }


        void GET_TRAINING_DOCUMENTS()
        {


            var srlNo = Request.QueryString["SrlNo"];
            var empCode = Request.QueryString["empCode"];


            var dt = new DataTable();

            dt.Columns.Add("SrlNo");
            dt.Columns.Add("FileName");
            dt.Columns.Add("RelativePath");
            dt.Columns.Add("DeletePath");

            try { Directory.CreateDirectory(Server.MapPath("~/HR/Docs/Education/" + empCode)); } catch { }

            try { Directory.CreateDirectory(Server.MapPath("~/HR/Docs/Education/" + empCode + "/" + srlNo)); } catch { }

            var files = Directory.GetFiles(Server.MapPath("~/HR/Docs/Education/" + empCode + "/" + srlNo));

            for (int i = 0; i < files.Length; i++)
            {

                var fileInf = new FileInfo(files[i]);

                var row = dt.NewRow();

                row["SrlNo"] = i + 1;
                row["FileName"] = fileInf.Name;
                row["RelativePath"] = ResolveUrl("~/HR/Docs/Education/" + empCode + "/" + srlNo) + "/" + fileInf.Name;
                row["DeletePath"] = ("~/HR/Docs/Education/" + empCode + "/" + srlNo) + "/" + fileInf.Name;


                dt.Rows.Add(row);


            }


            var JSONresult = JsonConvert.SerializeObject(dt);
            Response.Write(JSONresult);
            //return JSONresult;
        }

        void GET_TRAVEL_EXP_DOCS()
        {


            var srlNo = Request.QueryString["SrlNo"];
            var expRowId = Request.QueryString["ExpSaveId"];
            var rootPath = MapPath(@"~/Ess/Docs");


            var dt = new DataTable();

            dt.Columns.Add("SrlNo");
            dt.Columns.Add("FileName");
            dt.Columns.Add("RelativePath");
            dt.Columns.Add("DeletePath");
            var FOLDERPATH = "";
            if (expRowId == "-1")
            {
                FOLDERPATH = rootPath + "/Travel-Exp/" + Session.SessionID + "/" + srlNo;
            }
            else
            {
                FOLDERPATH = rootPath + "/Travel-Exp/" + expRowId + "/";
            }
            if (!Directory.Exists(FOLDERPATH))
                return;

            var files = Directory.GetFiles(FOLDERPATH);

            for (int i = 0; i < files.Length; i++)
            {

                var fileInf = new FileInfo(files[i]);

                var row = dt.NewRow();

                row["SrlNo"] = i + 1;
                row["FileName"] = fileInf.Name;
                row["RelativePath"] = ResolveUrl(@"~/Ess/Docs/Travel-Exp/" + (expRowId == "-1" ? Session.SessionID : expRowId)) + "/" + fileInf.Name;
                row["DeletePath"] = (@"~/Ess/Docs/Travel-Exp/" + (expRowId == "-1" ? Session.SessionID : expRowId)) + "/" + fileInf.Name;

                 dt.Rows.Add(row);


            }


            var JSONresult = JsonConvert.SerializeObject(dt);
            Response.Write(JSONresult);
            //return JSONresult;
        }

        void QUAL_DOC_UPLOAD()
        {

            var files = Request.Files;

            var srlNo = Request.QueryString["SrlNo"];
            var empCode = Request.QueryString["empCode"];

            try { Directory.CreateDirectory(Server.MapPath("~/HR/Docs/Education/" + empCode)); } catch { }

            try { Directory.CreateDirectory(Server.MapPath("~/HR/Docs/Education/" + empCode + "/" + srlNo)); } catch { }


            for (int i = 0; i < Request.Files.Count; i++)
            {
                HttpPostedFile fileUpload = Request.Files[i];

                fileUpload.SaveAs(Server.MapPath("~/HR/Docs/Education/" + empCode + "/" + srlNo + "/" + fileUpload.FileName));

            }

            WriteSuccessMessage("Files saved successfully");
        }


        void TRAINING_DOC_UPLOAD()
        {

            var files = Request.Files;

            var srlNo = Request.QueryString["SrlNo"];
            var empCode = Request.QueryString["empCode"];

            try { Directory.CreateDirectory(Server.MapPath("~/HR/Docs/Education/" + empCode)); } catch { }

            try { Directory.CreateDirectory(Server.MapPath("~/HR/Docs/Education/" + empCode + "/" + srlNo)); } catch { }
            //Request URL: http://localhost:8085/ESS-AER/ESSHomeData.aspx?Method=TRAINING_DOC_UPLOAD&empCode=OM10001&SrlNo=72


            // for (int i = 0; i < Request.Files.Count; i++)
            HttpPostedFile fileUpload = Request.Files["fileInputtrain"];

            if (fileUpload != null)
                fileUpload.SaveAs(Server.MapPath("~/HR/Docs/Education/" + empCode + "/" + srlNo + "/" + fileUpload.FileName));


            WriteSuccessMessage("Files saved successfully");
        }
        void WARNING_VIEW()
        {

            var query = @"

SELECT 
    HWG_NO, HWG_TYPE, DECODE(HWG_TYPE,'W', 'Warning' , 'G' , 'Grievance' , 'B' , 'Both') TYPE_NAME, 
    HWG_ISSUED_BY,  EMP1.ENAME ISSUEDEMP_NAME,
    HWG_ISSUED_DATE, TO_CHAR(HWG_ISSUED_DATE,'dd/MM/yyyy') ISSUED_DATE , HWG_AGAINST_EMPLOYEE, EMP2.ENAME  AGAINSTEMP_NAME, HWG_CAT_CODE, HWGC_DESCRIPTION ,
    HWG_SUMMARY, HWG_IMPACT, HWG_FUTURE_EXPECTATIONS, 
    HWG_CONS_FUTURE_VIOLATIONS, HWG_STATUS , DECODE(HWG_STATUS, 'N' ,'New' , 'P' , 'Posted') STATUS
FROM HRT_WARNING_GRIEVANCE ,HRM_WAR_GRI_CATEGORY , VU_EMP_DET EMP1 , VU_EMP_DET EMP2 
WHERE HWG_CAT_CODE = HWGC_CAT_CODE 
AND EMP1.EMPID = HWG_ISSUED_BY
AND EMP2.EMPID =  HWG_AGAINST_EMPLOYEE  
and HWG_AGAINST_EMPLOYEE ='" + Request.QueryString["EmpCode"] + @"'
";

            //AppCode.LogError.WriteToLogFile("", true, query);


            var dt1 = db.execute_query_retun_datatable(query);

            var JSONresult = JsonConvert.SerializeObject(dt1);
            Response.Write(JSONresult);

        }

        void EDU_DELETE()
        {


            var query = @"Delete from PPT_EMP_EDUCATION_DETAILS where PEED_EDUCATION_SRL_NO ='" + Request.QueryString["PEED_EDUCATION_SRL_NO"] + @"' ";

            if (db.execute_query(query) > 0)
            {
                WriteSuccessMessage("Deleted Successfully.");
            }
            else
                ResponseWrite("Error: Unable to delete .");


        }
        private DataTable intialize_grid_S()
        {

            var dt_details = new DataTable("dt_details");


            dt_details.Columns.Add("PEED_EDUCATION_SRL_NO");
            dt_details.Columns.Add("PEED_EDUCATION_INST_NAME");
            dt_details.Columns.Add("PEED_EDUCATION_TYPE_FLAG");
            dt_details.Columns.Add("PEED_EDUCATION_DEGREE");
            dt_details.Columns.Add("PEED_EDUCATION_JOIN_DATE");
            dt_details.Columns.Add("PEED_EDUCATION_AWARD_DATE");
            dt_details.Columns.Add("PEED_EDUCATION_REMARKS");
            dt_details.Columns.Add("chk_delete");

            return dt_details;
        }
        void EDU_SAVE()
        {
            var totRecords = 1;// Request.Form["totDepRows"].ToInt();
            dbAccessXML obj_db_xml = new dbAccessXML();

            if (totRecords <= 0)
            {
                ResponseWrite("Error: nothing to save.");
                return;
            }
            // validate

            var dt_details = intialize_grid_S();

            for (int i = 0; i < 50; i++)
            {
                var j = i + 1;
                if (string.IsNullOrEmpty(Request.Form["PEED_EDUCATION_INST_NAME" + j]))
                {
                    continue;
                }
                DataRow dr = dt_details.NewRow();
                dr["PEED_EDUCATION_SRL_NO"] = Request.Form["PEED_EDUCATION_SRL_NO" + j];
                dr["PEED_EDUCATION_INST_NAME"] = Request.Form["PEED_EDUCATION_INST_NAME" + j];
                dr["PEED_EDUCATION_TYPE_FLAG"] = Request.Form["PEED_EDUCATION_TYPE_FLAG" + j];
                dr["PEED_EDUCATION_DEGREE"] = Request.Form["PEED_EDUCATION_DEGREE" + j];
                dr["PEED_EDUCATION_JOIN_DATE"] = Request.Form["PEED_EDUCATION_JOIN_DATE" + j];
                dr["PEED_EDUCATION_AWARD_DATE"] = Request.Form["PEED_EDUCATION_AWARD_DATE" + j];
                dr["PEED_EDUCATION_REMARKS"] = Request.Form["PEED_EDUCATION_REMARKS" + j];



                dt_details.Rows.Add(dr);


            }

            StringWriter obj_stream_writer = new StringWriter();
            dt_details.TableName = "dt_details";
            dt_details.WriteXml(obj_stream_writer);

            string str_result = "";
            //obj_db_xml.execute_xml_procedure_emp_dependent(obj_stream_writer.ToString(), Request.Form["EMPCode"].Trim(), Request.Form["UserId"], "N", out str_result);
            obj_db_xml.execute_xml_procedure2(obj_stream_writer.ToString(), Request.Form["EMPCode"].Trim(), Request.Form["UserId"], "N", out str_result);


            //AppCode.LogError.WriteToLogFile("", true, obj_stream_writer.ToString());

            if (str_result != null && str_result.Contains("ORA-"))
            {
                ResponseWrite("Error:" + str_result);

            }
            else
                WriteSuccessMessage(str_result);



        }


        void TRAINING_SAVE()
        {
            var totRecords = 1;// Request.Form["totDepRows"].ToInt();
            dbAccessXML obj_db_xml = new dbAccessXML();
            List<string> qu = new List<string>();
            if (totRecords <= 0)
            {
                ResponseWrite("Error: nothing to save.");
                return;
            }
            // validate

            var dt_details = intialize_grid_Training();

            for (int i = 0; i < 50; i++)
            {
                var j = i + 1;
                var PETD_TRAINING_NAME = Request.Form["PETD_TRAINING_NAME" + j];
                if (string.IsNullOrEmpty(PETD_TRAINING_NAME))
                {
                    continue;
                }
                // var CHECK = Request.Form["PDPD_CHK_DELETE" + j];
                DataRow dr = dt_details.NewRow();
                dr["PETD_TRAINING_SRLNO"] = Request.Form["TrainSRNo" + j];

                var PETD_TRAINING_PLACE = Request.Form["PETD_TRAINING_PLACE" + j];
                var PETD_TRAINING_FROM_DATE = Request.Form["TrainFrom" + j];
                var PETD_TRAINING_TO_DATE = Request.Form["TrainTo" + j];
                var PETD_TRAINING_REMARKS = Request.Form["TrainRemarks" + j];
                // var CHKISDELETE = "N"; //(Request.Form["PDPD_CHK_DELETE" + j] == "on" ? "Y" : "N");
                var ACM_COUNTRY_CODE = Request.Form["ddlcountry" + j];
                var srlNo = Request.Form["TrainSRNo" + j];



                if (srlNo.ToInt() > 0)
                {
                    // update
                    qu.Add(@"
                    UPDATE PPT_EMP_TRAINING_DETAILS
                  SET PETD_TRAINING_NAME = '" + PETD_TRAINING_NAME.Replace("'", "''") + @"',
                      PETD_TRAINING_PLACE ='" + PETD_TRAINING_PLACE.Replace("'", "''") + @"',
                      PETD_TRAINING_COUNTRY='" + ACM_COUNTRY_CODE.Replace("'", "''") + @"',
                      PETD_TRAINING_FROM_DATE =to_date( '" + PETD_TRAINING_FROM_DATE + @"','dd/mm/yyyy'),
                      PETD_TRAINING_TO_DATE = to_date( '" + PETD_TRAINING_TO_DATE + @"','dd/mm/yyyy'),
                      PETD_UPDATE_USER_ID = '" + user.User_Id + @"',
                      PETD_UPDATE_DATE = SYSDATE,
                      PETD_TRAINING_REMARKS =  '" + PETD_TRAINING_REMARKS.Replace("'", "''") + @"'
                      WHERE PETD_TRAINING_EMP_CODE = '" + user.Emp_Code + @"'
                      AND PETD_TRAINING_SRLNO = " + dr["PETD_TRAINING_SRLNO"] + @"");
                }
                else
                {

                    //INSERT
                    var sqlquery = @"Select count(*) from PPT_EMP_TRAINING_DETAILS where PETD_TRAINING_EMP_CODE ='" + Request.Form["EMPCode"].Trim() + @"'
                      and(PETD_TRAINING_FROM_DATE >=to_date( '" + PETD_TRAINING_FROM_DATE + @"','dd/mm/yyyy')
                     and PETD_TRAINING_TO_DATE <= to_date( '" + PETD_TRAINING_TO_DATE + @"','dd/mm/yyyy'))";

                    if (db.execute_scalar(sqlquery).ToInt() > 0)
                    {
                        ResponseWrite("Error:" + "Already Exist this date."); return;
                    }

                    sqlquery = @"Select count(*) from PPT_EMP_TRAINING_DETAILS where PETD_TRAINING_EMP_CODE ='" + Request.Form["EMPCode"].Trim() + @"' and PETD_TRAINING_NAME ='" + PETD_TRAINING_NAME + @"'";
                    if (db.execute_scalar(sqlquery).ToInt() > 0)
                    {
                        ResponseWrite("Error:" + "Already Exist this Training Name."); return;
                    }

                    qu.Add(@" INSERT INTO PPT_EMP_TRAINING_DETAILS
                              (PETD_TRAINING_SRLNO,PETD_TRAINING_EMP_CODE,
                               PETD_TRAINING_NAME, 
                               PETD_TRAINING_PLACE,
                               PETD_TRAINING_COUNTRY,
                               PETD_TRAINING_FROM_DATE,
                               PETD_TRAINING_TO_DATE,
                               PETD_CREATION_USER_ID,
                               PETD_CREATION_DATE,
                               PETD_TRAINING_REMARKS                              
                              )
                       VALUES (
                                seq_emp_training.NEXTVAL,
                            '" + user.Emp_Code + @"',
                            '" + PETD_TRAINING_NAME.Replace("'", "''") + @"',
                            '" + PETD_TRAINING_PLACE.Replace("'", "''") + @"',
                            '" + ACM_COUNTRY_CODE.Replace("'", "''") + @"',  
                            to_date( '" + PETD_TRAINING_FROM_DATE + @"','dd/mm/yyyy'),
                           to_date( '" + PETD_TRAINING_TO_DATE + @"','dd/mm/yyyy'),                              
                                '" + user.User_Id + @"',
                                SYSDATE,
                            '" + PETD_TRAINING_REMARKS.Replace("'", "''") + @"'
                              )");
                }


            }

            var statuss = objDB.executeQueryInTrasnaction(qu);

            if (!statuss)
            {
                ResponseWrite("Error:" + objDB.error_msg);

            }
            else
                WriteSuccessMessage("Successfully saved.");



        }

        private DataTable intialize_grid_Training()
        {

            var dt_details = new DataTable("Training_details");


            dt_details.Columns.Add("PETD_TRAINING_SRLNO");
            dt_details.Columns.Add("PETD_TRAINING_NAME");
            dt_details.Columns.Add("PETD_TRAINING_PLACE");
            dt_details.Columns.Add("PETD_TRAINING_FROM_DATE");
            dt_details.Columns.Add("PETD_TRAINING_TO_DATE");
            dt_details.Columns.Add("PETD_TRAINING_REMARKS");
            dt_details.Columns.Add("ACM_COUNTRY_CODE");
            dt_details.Columns.Add("chk_delete");

            return dt_details;
        }

        void TRAINING_DELETE()
        {


            var query = @"Delete from PPT_EMP_TRAINING_DETAILS where PETD_TRAINING_SRLNO ='" + Request.QueryString["TRAINING_SRL_NO"] + @"' ";
            var status = objDB.execute_query(query);
            Response.Write("Deleted Successfully.");
         

        }


        void HISTORY_SAVE()
        {
            var totRecords = 1;// Request.Form["totDepRows"].ToInt();
            dbAccessXML obj_db_xml = new dbAccessXML();
            List<string> qu = new List<string>();
            if (totRecords <= 0)
            {
                ResponseWrite("Error: nothing to save.");
                return;
            }
            // validate

            var dt_details = intialize_grid_History();

            for (int i = 0; i < 50; i++)
            {
                var j = i + 1;
                if (string.IsNullOrEmpty(Request.Form["PEEH_EMPMNT_HIST_FROM_DATE" + j]))
                {
                    continue;
                }
                DataRow dr = dt_details.NewRow();
                dr["PEEH_EMPMNT_HIST_SRLNO"] = Request.Form["PEEH_EMPMNT_HIST_SRLNO" + j];
                var PEEH_EMPMNT_HIST_FROM_DATE = Request.Form["PEEH_EMPMNT_HIST_FROM_DATE" + j];
                var PEEH_EMPMNT_HIST_TO_DATE = Request.Form["PEEH_EMPMNT_HIST_TO_DATE" + j];
                var PEEH_EMPMNT_HIST_ORG = Request.Form["PEEH_EMPMNT_HIST_ORG" + j];
                var PEEH_EMPMNT_HIST_PLACE = Request.Form["PEEH_EMPMNT_HIST_PLACE" + j];
                var PEEH_EMPMNT_HIST_DESG = Request.Form["PEEH_EMPMNT_HIST_DESG" + j];
                var PEEH_EMPMNT_HIST_REMARKS = Request.Form["PEEH_EMPMNT_HIST_REMARKS" + j];
                var PEEH_COUNTRY_CODE = Request.Form["ACM_COUNTRY_CODE" + j];
                //dr["chk_delete"] = "N"; // (Request.Form["PDPD_CHK_DELETE" + j] == "on" ? "Y" : "N");               
                var srlNo = Request.Form["PEEH_EMPMNT_HIST_SRLNO" + j];

                //i_loop++;

                if (srlNo.ToInt() > 0)
                {
                    // update
                    qu.Add(@"
                    UPDATE PPT_EMP_EMPLOYEMENT_HISTORY
                  SET PEEH_EMPMNT_HIST_FROM_DATE = to_date( '" + PEEH_EMPMNT_HIST_FROM_DATE + @"','dd/mm/yyyy'),
                      PEEH_EMPMNT_HIST_TO_DATE = to_date( '" + PEEH_EMPMNT_HIST_TO_DATE + @"','dd/mm/yyyy'),
                      PEEH_EMPMNT_HIST_ORG = '" + PEEH_EMPMNT_HIST_ORG.Replace("'", "''") + @"',
                      PEEH_EMPMNT_HIST_PLACE ='" + PEEH_EMPMNT_HIST_PLACE.Replace("'", "''") + @"',
                      PEEH_EMPMNT_HIST_DESG = '" + PEEH_EMPMNT_HIST_DESG.Replace("'", "''") + @"',
                      PEEH_EMPMNT_HIST_REMARKS = '" + PEEH_EMPMNT_HIST_REMARKS.Replace("'", "''") + @"',
                      PEEH_COUNTRY_CODE= '" + PEEH_COUNTRY_CODE + @"',
                      PEEH_UPDATE_DATE = SYSDATE,
                      PEEH_UPDATE_USER_ID ='" + user.User_Id + @"'                     
                      WHERE PEEH_EMP_CODE = '" + user.Emp_Code + @"'
                      AND PEEH_EMPMNT_HIST_SRLNO = " + dr["PEEH_EMPMNT_HIST_SRLNO"] + @"");
                }
                else
                {

                    //INSERT

                    var sqlquery = @"Select count(*) from PPT_EMP_EMPLOYEMENT_HISTORY where PEEH_EMP_CODE ='" + Request.Form["EMPCode"].Trim() + @"'
                      and(PEEH_EMPMNT_HIST_FROM_DATE >=to_date( '" + Request.Form["PEEH_EMPMNT_HIST_FROM_DATE" + j] + @"','dd/mm/yyyy')
                     and PEEH_EMPMNT_HIST_TO_DATE <= to_date( '" + Request.Form["PEEH_EMPMNT_HIST_TO_DATE" + j] + @"','dd/mm/yyyy'))";

                    if (db.execute_scalar(sqlquery).ToInt() > 0)
                    {
                        ResponseWrite("Error:" + "Already Exist this History."); return;
                    }



                    qu.Add(@" INSERT INTO PPT_EMP_EMPLOYEMENT_HISTORY
                              (PEEH_EMPMNT_HIST_SRLNO,PEEH_EMP_CODE,
                               PEEH_EMPMNT_HIST_FROM_DATE, 
                               PEEH_EMPMNT_HIST_TO_DATE,
                               PEEH_EMPMNT_HIST_ORG,
                               PEEH_EMPMNT_HIST_PLACE,
                               PEEH_EMPMNT_HIST_DESG,
                               PEEH_EMPMNT_HIST_REMARKS,
                               PEEH_COUNTRY_CODE,
                               PEEH_CREATION_DATE,
                               PEEH_CREATION_USER_ID                              
                              )
                       VALUES (
                                SEQ_EMP_HISTORY.NEXTVAL,
                            '" + user.Emp_Code + @"',
                            to_date( '" + PEEH_EMPMNT_HIST_FROM_DATE + @"','dd/mm/yyyy'),
                            to_date( '" + PEEH_EMPMNT_HIST_TO_DATE + @"','dd/mm/yyyy'),
                           '" + PEEH_EMPMNT_HIST_ORG.Replace("'", "''") + @"',  
                           '" + PEEH_EMPMNT_HIST_PLACE.Replace("'", "''") + @"',
                           '" + PEEH_EMPMNT_HIST_DESG.Replace("'", "''") + @"',
                           '" + PEEH_EMPMNT_HIST_REMARKS.Replace("'", "''") + @"',
                           '" + PEEH_COUNTRY_CODE + @"',
                               SYSDATE,  
                              '" + user.User_Id + @"'                           
                              )");
                }

                dt_details.Rows.Add(dr);


            }

            var statuss = objDB.executeQueryInTrasnaction(qu);


            if (!statuss)
            {
                ResponseWrite("Error:" + objDB.error_msg);

            }
            else
                WriteSuccessMessage("Successfully saved.");




        }

        private DataTable intialize_grid_History()
        {

            var dt_details = new DataTable("dt_details");
            dt_details.Columns.Add("PEEH_EMPMNT_HIST_SRLNO");
            dt_details.Columns.Add("PEEH_EMPMNT_HIST_FROM_DATE");
            dt_details.Columns.Add("PEEH_EMPMNT_HIST_TO_DATE");
            dt_details.Columns.Add("PEEH_EMPMNT_HIST_ORG");
            dt_details.Columns.Add("PEEH_EMPMNT_HIST_PLACE");
            dt_details.Columns.Add("PEEH_EMPMNT_HIST_DESG");
            dt_details.Columns.Add("PEEH_EMPMNT_HIST_REMARKS");
            dt_details.Columns.Add("chk_delete");
            dt_details.Columns.Add("ACM_COUNTRY_CODE");

            return dt_details;
        }
        void HISTORY_DELETE()
        {


            var query = @"Delete from PPT_EMP_EMPLOYEMENT_HISTORY where PEEH_EMPMNT_HIST_SRLNO ='" + Request.QueryString["PEEH_EMPMNT_HIST_SRLNO"] + @"' ";

            var status = db.execute_query(query);

            WriteSuccessMessage("Deleted Successfully.");

        }



        void DEPENDANT_DELETE()
        {
            var query = @"Delete from PPT_EMP_DEPENDENT_DETAILS where PDPD_DEPENDENT_CODE ='" + Request.QueryString["SRLNO"] + @"' ";

            if (db.execute_query(query) > 0)
            {
                WriteSuccessMessage("Deleted Successfully.");
            }
            else
                ResponseWrite("Error: Unable to delete .");


        }
        private DataTable intialize_grid()
        {

            var dt_details = new DataTable("dt_details");

            dt_details.Columns.Add("PDPD_DEPENDENT_CODE");
            dt_details.Columns.Add("PDPD_DEPENDENT_TITLE");
            dt_details.Columns.Add("PDPD_DEPENDENT_NAME");
            dt_details.Columns.Add("PDPD_DEPENDENT_SEX");
            dt_details.Columns.Add("PDPD_DEPENDENT_RELATION");
            dt_details.Columns.Add("PDPD_DEPENDENT_DATE_BIRTH");
            dt_details.Columns.Add("PDPD_DEPENDENT_COMP_INSURED");
            dt_details.Columns.Add("PDPD_DEPENDENT_AIRFARE_YN");
            dt_details.Columns.Add("PDPD_DEPENDENT_VISA_ISSUES_YN");
            dt_details.Columns.Add("PDPD_REMARKS");
            dt_details.Columns.Add("chk_delete");
            dt_details.Columns.Add("RELATION_DESCRIPTION");
            dt_details.Columns.Add("PDPD_DEPENDENT_TYPE");
            dt_details.Columns.Add("PDPD_GSM");
            dt_details.Columns.Add("PDPD_LIB_PERC");
            dt_details.Columns.Add("PDPD_DEPENDENT_DATE_EXPIRY");
            return dt_details;

        }
        void DEPENDANT_SAVE()
        {
            var totRecords = Request.Form["totDepRows"].ToInt();
            dbAccessXML obj_db_xml = new dbAccessXML();

            if (totRecords <= 0)
            {
                ResponseWrite("Error: nothing to save.");
                return;
            }
            // validate

            var dt_details = intialize_grid();

            for (int i = 0; i < 50; i++)
            {

                var j = i + 1;

                if (string.IsNullOrEmpty(Request.Form["depFullName" + j]))
                {
                    continue;
                }
                DataRow dr = dt_details.NewRow();

                dr["PDPD_DEPENDENT_CODE"] = Request.Form["depSRNo" + j];
                dr["PDPD_DEPENDENT_TITLE"] = Request.Form["PDPD_DEPENDENT_TITLE" + j];
                dr["PDPD_DEPENDENT_NAME"] = Request.Form["depFullName" + j];
                dr["PDPD_DEPENDENT_SEX"] = Request.Form["depGender" + j];
                dr["PDPD_DEPENDENT_RELATION"] = Request.Form["depRelationType" + j];
                dr["PDPD_DEPENDENT_DATE_BIRTH"] = Request.Form["depDOB" + j];
                dr["PDPD_DEPENDENT_TYPE"] = Request.Form["depType" + j];
              //  depEXPIRY
                dr["PDPD_GSM"] = Request.Form["PDPD_GSM" + j];
                dr["PDPD_LIB_PERC"] = Request.Form["PDPD_LIB_PERC" + j];
                dr["PDPD_DEPENDENT_DATE_EXPIRY"] = Request.Form["depEXPIRY" + j];
                dr["PDPD_DEPENDENT_COMP_INSURED"] = (Request.Form["PDPD_DEPENDENT_COMP_INSURED" + j] == "on" ? "Y" : "N");
                dr["PDPD_DEPENDENT_AIRFARE_YN"] = (Request.Form["PDPD_DEPENDENT_AIRFARE_YN" + j] == "on" ? "Y" : "N");
                dr["PDPD_DEPENDENT_VISA_ISSUES_YN"] = (Request.Form["PDPD_DEPENDENT_VISA_ISSUES_YN" + j] == "on" ? "Y" : "N");


                dt_details.Rows.Add(dr);


            }

            StringWriter obj_stream_writer = new StringWriter();
            dt_details.TableName = "dt_details";
            dt_details.WriteXml(obj_stream_writer);
            //AppCode.LogError.WriteToLogFile("", false, obj_stream_writer.ToString());

            string str_result = "";
            obj_db_xml.execute_xml_procedure_emp_dependent(obj_stream_writer.ToString(), Request.Form["EMPCode"].Trim(), Request.Form["UserId"], "N", out str_result);
            //  AppCode.LogError.WriteToLogFile("result..", true, str_result.ToString());

            if (str_result != null && str_result.Contains("ORA-"))
            {
                ResponseWrite("Error:" + str_result);

            }
            else if (str_result.Trim().Equals("null"))
                ResponseWrite("Error: Enter data to save.");
            else
                WriteSuccessMessage(str_result);



        }
        void SECTOR()
        {

            var query = @"
SELECT  PSCM_SECTOR_COUNTRY, PSCM_SECTOR_CODE, PSCM_SECTOR_NAME FROM PPM_SECTOR_MASTER where PSCM_SECTOR_COUNTRY = '" + Request.QueryString["COUNTRY"] + @"'

";

            var dt = db.execute_query_retun_datatable(query);

            var JSONresult = JsonConvert.SerializeObject(dt);
            Response.Write(JSONresult);
            //return JSONresult;
        }
        void REPORTING_MANAGER()
        {

            var query = @"
select PEMP_EMP_CODE, PEMP_EMP_NAME from PPM_EMPLOYEE_DETAILS where pemp_emp_Code <> '" + Request.QueryString["EmpCode"] + @"'
  ORDER BY TO_NUMBER(REGEXP_REPLACE(PEMP_EMP_CODE, '[^0-9]+', '')) ASC";


            //AppCode.LogError.WriteToLogFile("", true, query);

            var dt = db.execute_query_retun_datatable(query);

            var JSONresult = JsonConvert.SerializeObject(dt);
            Response.Write(JSONresult);
            //return JSONresult;
        }
        void SUBGRADE()
        {

            var query = @"
SELECT  PSGD_SUB_GRADE_CODE, PSGD_SUB_GRADE_DESC FROM PPM_SUB_GRADE_MASTER

";

            var dt = db.execute_query_retun_datatable(query);

            var JSONresult = JsonConvert.SerializeObject(dt);
            Response.Write(JSONresult);
            //return JSONresult;
        }
        void GRADE()
        {

            var query = @"
SELECT  PGDM_GRADE_CODE, PGDM_GRADE_DESC FROM PPM_GRADE_MASTER 
";

            var dt = db.execute_query_retun_datatable(query);

            var JSONresult = JsonConvert.SerializeObject(dt);
            Response.Write(JSONresult);
            //return JSONresult;
        }
        void DIVISION()
        {

            var query = @"
SELECT 	PDM_DIV_CODE, PDM_DIV_NAME FROM PPM_DIVISION_MASTER


";

            var dt = db.execute_query_retun_datatable(query);

            var JSONresult = JsonConvert.SerializeObject(dt);
            Response.Write(JSONresult);
            //return JSONresult;
        }

        void DESIGNATION()
        {

            var query = @"
SELECT 
    PDSM_DESIGNATION_CODE, PDSM_DESIGNATION_DESC
FROM PPM_DESIGNATION_MASTER

";
            //AppCode.LogError.WriteToLogFile("", true, query);

            var dt = db.execute_query_retun_datatable(query);

            var JSONresult = JsonConvert.SerializeObject(dt);
            Response.Write(JSONresult);
            //return JSONresult;
        }

        void GetMangerDetails()
        {
            var EMPCode = Request.QueryString["EMPid"];

            var strquery = @"Select * from ppm_employee_details WHERE 1=1 
                  AND pemp_emp_code='" + EMPCode + "'";
            var dt = db.execute_query_retun_datatable(strquery);

            var JSONresult = JsonConvert.SerializeObject(dt);
            Response.Write(JSONresult);

        }
        void EMP_DET_SAVE()
        {
            var EMPCode = Request.Form["EMPCode"];
            var empName = Request.Form["empName"];
            var email = Request.Form["email"];
            var address1 = Request.Form["address1"];
            var address2 = Request.Form["address2"];
            var empPhone = Request.Form["empPhone"];
            var empMaritalStatus = Request.Form["empMaritalStatus"];
            var dob = Request.Form["dob"];
            var empPlaceOfBirth = Request.Form["empPlaceOfBirth"];
            var desigCode = Request.Form["desigCode"];
            var divCode = Request.Form["divCode"];
            var gradeCode = Request.Form["gradeCode"];
            var subGradeCode = Request.Form["subGradeCode"];
            var reportingManagerCode = Request.Form["reportingManagerCode"];
            var actingManagerCode = Request.Form["actingManagerCode"];
            var airSectorCode = Request.Form["airSectorCode"];
            var relativeName = Request.Form["relativeName"];
            var ddlRelationWithEMp = Request.Form["ddlRelationWithEMp"];
            var relativePhoneNo = Request.Form["relativePhoneNo"];
            var relativeAddress1 = Request.Form["relativeAddress1"];
            var relativeAddress2 = Request.Form["relativeAddress2"];

            var updateQuery = @"
update ppm_employee_Details set 
    PEMP_EMP_PHONE ='" + empPhone.Replace("'", "''") + @"',
    PEMP_EMP_SOCIAL_STATUS ='" + empMaritalStatus + @"',
PEMP_EMP_NEXT_TO_KIN= '" + relativeName + @"',
PEMP_EMP_NKT_RELATION= '" + ddlRelationWithEMp + @"',
PEMP_EMP_NKT_PHONE= '" + relativePhoneNo + @"',
PEMP_EMP_NKT_ADDRESS_1= '" + relativeAddress1 + @"',
PEMP_EMP_NKT_ADDRESS_2= '" + relativeAddress2 + @"',
PEMP_ACTING_MNGR='" + actingManagerCode + @"'
where
    pemp_emp_Code='" + EMPCode + @"' 
";

            if (db.execute_query(updateQuery) > 0)
            {
                WriteSuccessMessage("Data Saved Successfully.");
            }
            else
            {
                ResponseWrite("Error: Unable to save.");

            }
        }
        void PAYSLIP()
        {

            var empCode = Request.QueryString["EmpCode"];
            var year = Request.QueryString["year"].ToInt();
            var month = Request.QueryString["moth"].ToInt();
            DateTime date1 = new DateTime(year, month, 1);

            /// check group payroll done for the month or not.
            var groupFinalDone = db.record_found(@"
SELECT 
    Count(1) 
FROM
    PPT_PAY_PROCESS_GROUP_STATUS, PPM_EMP_PAY_GROUP_MEMBERS
where
    PEPGM_EMP_CODE  ='" + empCode + @"'  
    and PPGS_GROUP_CODE= PEPGM_GROUP_CODE
    and  PPGS_YEAR_MONTH = '" + date1.ToString("yyyyMM").Trim() + @"'
    and PPGS_PROCESS_CURR_STATUS ='F' 
") > 0;

            if (!groupFinalDone)
            {
                var groupName = db.execute_scalar(@"

SELECT 
     PEPPG_GROUP_DESC
FROM
   PPM_EMP_PAY_GROUP_MEMBERS,
   PPM_EMP_PAY_PROCESS_GROUP
where
    PEPGM_EMP_CODE  ='" + empCode + @"'  
    and PEPGM_GROUP_CODE = PEPPG_GROUP_CODE

");
                //ks.showAlert("alert('Final payroll not done for the group " + groupName + "');", Page);
                var dt2 = new DataTable();
                dt2.Columns.Add("Error");
                var row = dt2.NewRow();
                row["Error"] = "Final payroll not done for the group " + groupName;
                dt2.Rows.Add(row);
                dt2.AcceptChanges();

                var JSONresult2 = JsonConvert.SerializeObject(dt2);
                Response.Write(JSONresult2);
                return;

            }


            var query = @"
SELECT 
    V.*, CASE WHEN V.PEDM_EARN_DED_TYPE = 'E' THEN 'Earning' ELSE 'Deduction' END EAR_DED_TYPE_DESC, to_char(PPHD_AMOUNT,'9999.999') PPHD_AMOUNT1,
    NVL(PPHD_AMOUNT,0) PPHD_AMOUNT2
    
FROM V_EMP_EARN_DEDUCT_MONTH_DETL V   
    WHERE  
PEMP_EMP_ACTIVE='Y' AND 
NVL(PEMP_EMP_TERM_PROCESSED_IND,'N')='N' AND
NVL(PEMP_ANN_LEAVE_INDICATOR,'N')='N' 
    AND  PEMP_EMP_COMPANY_CODE='01'  
 and PEMP_EMP_CODE ='" + empCode + @"'
 and PPHD_PAY_YEAR_MONTH = '" + year + (month.ToString("00")) + @"'
order by PEDM_EARN_DED_TYPE desc, PEDM_SORT_ORDER
";
            AppCode.LogError.WriteToLogFile("PAYSLIP", true, query);

            var dt = db.execute_query_retun_datatable(query);
            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow item in dt.Rows)
                {
                    item["PERD_DEDUC_EARN_REMARKS"] = date1.ToString("MMM-yyyy", new CultureInfo("en-US"));
                }

                dt.AcceptChanges();

                var earning = dt.Compute("Sum(PPHD_AMOUNT2)", "PEDM_EARN_DED_TYPE='E'").ToString().ToDouble();
                var ded = dt.Compute("Sum(PPHD_AMOUNT2)", "PEDM_EARN_DED_TYPE='D'").ToString().ToDouble();
                var totToAdd = earning - ded;

                var row1 = dt.NewRow();
                row1["PPHD_AMOUNT1"] = totToAdd.ToString("0.000");
                row1["PERD_DEDUC_EARN_REMARKS"] = "Total";

                dt.Rows.Add(row1);





            }

            if (dt.Rows.Count > 0)
            {
                var JSONresult = JsonConvert.SerializeObject(dt);
                Response.Write(JSONresult);
            }
            else
            {
                Response.Write("{}");
            }
            //return JSONresult;


        }

        public DataTable GetWFDocumentApproverDetails(long wfDetailRowId)
        {//to_char(AWDD_APPROVAL_DATE,'dd/mm/yyyy')
            return db.execute_query_retun_datatable(@"

SELECT 
    AWAL_WF_DET_ROW_ID, AWAL_COMP_CODE, AWAL_EMP_CODE,PEMP_EMP_NAME,'' Comments,
    AWDD_COMMENTS, to_char(AWDD_APPROVAL_DATE,'dd/mm/yyyy HH:MI') as AWDD_APPROVAL_DATE, AWDD_STATUS ,
    AWAL_OWNER_OF_WF, AWAL_IS_APPROVED_YN, AWAL_APPROVAL_COMMENT,
    AWDD_EMP_APPROVED_BY, AWDD_APP_REQ_FROM_ALL, case when AWAL_IS_APPROVED_YN ='Y' then 
   to_char(AWAL_UPDATE_DATE,'dd/mm/yyyy HH:MI')  
else null end  AWAL_UPDATE_DATE
FROM 
    AUTH_WF_APPROVER_LOG,PPM_EMPLOYEE_DETAILS,AUTH_WF_DOC_DETAIL
WHERE
    AWAL_WF_DET_ROW_ID= '" + wfDetailRowId + @"'  
    AND AWAL_EMP_CODE =PEMP_EMP_CODE
    and AWAL_COMP_CODE=  PEMP_EMP_COMPANY_CODE
    and AWDD_ROW_ID = AWAL_WF_DET_ROW_ID

");
        }


        void WF_STATUS()
        {

            var headId = Request.QueryString["ReqheadId"].ToLong();
            /*EAL_LEAVE_REQ_ID, 'Level-' ||  EAL_LEVEL_ID as EAL_LEVEL_ID_STR, case when MIN(EAL_LEVEL_COMP_YN) ='Y' then 'Compeleted' else 'Pending' end 
    as LEVELSTATUS ,EAL_LEVEL_ID,
    MIN(EAL_LEVEL_COMP_YN) as EAL_LEVEL_COMP_YN*/
            var tableheader = Workflow.GetWFDocumentStatusDetails(headId); //(headId);

            string strquery = "SELECT  NVL(TAP_PARAVALUE,'Y') from TAS_ATT_PARAM where TAP_PARANAME='SHOW_APP_GRP_DETAILS'";
            string showApproverInfo = db.execute_scalar(strquery);
            if (string.IsNullOrEmpty(showApproverInfo)) showApproverInfo = "Y";


            string liFirstLevel = @"";

            if (tableheader != null)
            {
                foreach (DataRow item in tableheader.Rows)
                {



                    var innerLi = "";

                    //var innerTable = Workflow.GetWFDocumentApproverDetails(item["AWDD_ROW_ID"].ToString().ToLong());
                    var innerTable = GetWFDocumentApproverDetails(item["AWDD_ROW_ID"].ToString().ToLong());
                    //AWAL_EMP_CODE
                    //AWDD_EMP_APPROVED_BY
                    var tickMark = "";
                    /*    AWAL_OWNER_OF_WF, AWAL_IS_APPROVED_YN, AWAL_APPROVAL_COMMENT,
                        AWDD_EMP_APPROVED_BY, AWDD_APP_REQ_FROM_ALL*/


                    foreach (DataRow innerRow in innerTable.Rows)
                    {
                        var approverClasss = "";
                        tickMark = "";
                        var tickOrCancelbutton = "";
                        var color = "";
                        if (item["AWDD_STATUS"].ToString() == "A")
                        {
                            tickOrCancelbutton = "fa-check-circle";
                            color = "green";
                        }
                        else if (item["AWDD_STATUS"].ToString() == "R")
                        {

                            tickOrCancelbutton = "fa-remove";
                            color = "red";
                        }
                        if (innerRow["AWDD_APP_REQ_FROM_ALL"].ToString() == "Y")
                        {
                            if (innerRow["AWAL_IS_APPROVED_YN"].ToString() == "Y")
                            {
                                approverClasss = "ApproverGreen";
                                tickMark = "<a href='#'><i class='fa fa-check-circle'> </i><a>";
                            }
                        }
                        else
                        {
                            if (item["AWDD_EMP_APPROVED_BY"].ToString() == innerRow["AWAL_EMP_CODE"].ToString())
                            {
                                approverClasss = "ApproverGreen";
                                tickMark = "<a href='#'><i class='fa " + tickOrCancelbutton + @"'> </i><a>";
                            }
                        }
                        if (showApproverInfo == "Y")
                            innerLi += @"<li class='" + approverClasss + @"'>" + tickMark + @"
                            " + innerRow["PEMP_EMP_NAME"].ToString()+" - " + innerRow["AWAL_EMP_CODE"].ToString() + @"
                            </li>";
                        else if (showApproverInfo == "N" && innerTable.Rows.Count == 1)
                        {

                            innerLi += @"<li class='" + approverClasss + @"'>" + tickMark + @"
                            " + innerRow["PEMP_EMP_NAME"].ToString() + " - " + innerRow["AWAL_EMP_CODE"].ToString() + @"
                            </li>";

                        }

                        if (innerRow["AWDD_APP_REQ_FROM_ALL"].ToString() == "Y")
                        {
                            if (innerRow["AWAL_IS_APPROVED_YN"].ToString() == "Y")
                            {
                                innerLi += @" <ul style='list-style:none;'><li style='color: " + color + @"'>Comments: " + innerRow["AWAL_APPROVAL_COMMENT"].ToString() + " </li><li style='color: " + color + @"'>Date: " + innerRow["AWAL_UPDATE_DATE"].ToString() + " </li></ul>";
                            }
                            else
                            {
                                innerLi += @" <ul style='list-style:none;'><li style='color: red;'>Pending </li></ul>";
                            }

                        }
                        else
                        {
                            if (item["AWDD_EMP_APPROVED_BY"].ToString() == innerRow["AWAL_EMP_CODE"].ToString())
                            {
                                //show approver comments
                                innerLi += @" <ul style='list-style:none;'><li style='color: " + color + @"'>Comments: " + innerRow["AWDD_COMMENTS"].ToString() + " </li><li style='color: " + color + @"'>Date: " + innerRow["AWDD_APPROVAL_DATE"].ToString() + " </li></ul>";
                            }
                        }
                        //if (strval == "N" && innerTable.Rows.Count > 1)
                        // innerLi = @"";


                    }
                    var approvedRow = "";



                    liFirstLevel += @"
<li class='" + approvedRow + @"'><a href='#'  class='" + approvedRow + @"'>" + item["AEG_DESCRIPTION"].ToString() + @" " + tickMark + @"</a>
                      <ul>
                        " + innerLi + @"
                    </ul>
                </li>
";
                    approvedRow = "";

                }


                WriteSuccessMessage(@"
                          <ul id='tree1'>
                            " + liFirstLevel + @"
                            </ul>
                        ".Replace("\"", @"\"""));


            }


        }

        string APPROVER_DETAIL_SERVICE(string empCode, int serviceType)
        {


            //var serviceType = Request.Form["ddlLeaveType"].ToInt();
            //var empCode = Request.Form["EMPCode"];


            List<AppCode.Pro_Parameters> parameters = new List<AppCode.Pro_Parameters>();
            parameters.Add(new AppCode.Pro_Parameters { _par_name = "P_ELR_SERVICE_TYPE", _par_value = ((int)(ESSServices.RequestForServices)) });
            parameters.Add(new AppCode.Pro_Parameters { _par_name = "P_ELR_SERVICE_REQ_TYPE", _par_value = serviceType });
            parameters.Add(new AppCode.Pro_Parameters { _par_name = "P_EMP_CODE", _par_value = empCode });
            var manager = new AppCode.Pro_Parameters { _par_name = "P_MANAGER_CODE", _par_value = "", _par_in_out = "OUT", _par_data_type = "Varchar2" };
            parameters.Add(manager);
            var managerName = new AppCode.Pro_Parameters { _par_name = "P_MANAGER_NAME", _par_value = "", _par_in_out = "OUT", _par_data_type = "Varchar2" };
            parameters.Add(managerName);
            var approvalGroup = new AppCode.Pro_Parameters { _par_name = "P_GROUP_NAME", _par_value = "", _par_in_out = "OUT", _par_data_type = "Varchar2" };
            parameters.Add(approvalGroup);

            AppCode.dbaccess db = new dbaccess();
            db.executeProcedure("ESS_GET_FIRST_LEVEL_MANAGER", parameters);


            if (manager._par_value == null || string.IsNullOrEmpty(manager._par_value.ToString()))
            {
                // get manager 
                return "Manager is not assigned, kindly contact admin";
            }

            //if (managerName._par_value != null && !string.IsNullOrEmpty(managerName._par_value.ToString()))//!string.IsNullOrEmpty(ESSUserDetail.GetESSUserDetailInstance.ManagerEmpCode))
            //{
            //    Response.Write("Will be approved by - " +
            //       approvalGroup._par_value.ToString() + ((managerName._par_value != null) ? "(" + managerName._par_value.ToString() + ")" : ""));
            //    //hiddenApprovalManagerCode.Value = 

            //}
            var apporvedBy = "";
            if (approvalGroup._par_value != null && !string.IsNullOrEmpty(approvalGroup._par_value.ToString()))
            {
                if (managerName._par_value != null && !string.IsNullOrEmpty(managerName._par_value.ToString()))//!string.IsNullOrEmpty(ESSUserDetail.GetESSUserDetailInstance.ManagerEmpCode))
                {
                    apporvedBy = managerName._par_value.ToString();

                }
                else
                {
                    apporvedBy = approvalGroup._par_value.ToString();// + ((managerName._par_value != null) ? "(" + managerName._par_value.ToString() + ")" : "");
                    //hiddenApprovalManagerCode.Value = 

                }

            }
            return "";
            //Response.Write("Will be approved by - " + apporvedBy);

            //Response.Write("Saved successfully.");
        }

        void APPROVER_DETAIL()
        {


            var serviceType = Request.Form["ddlLeaveType"].ToInt();
            var empCode = Request.Form["EMPCode"];


            List<AppCode.Pro_Parameters> parameters = new List<AppCode.Pro_Parameters>();
            parameters.Add(new AppCode.Pro_Parameters { _par_name = "P_ELR_SERVICE_TYPE", _par_value = ((int)(ESSServices.DocumentRequest)) });
            parameters.Add(new AppCode.Pro_Parameters { _par_name = "P_ELR_SERVICE_REQ_TYPE", _par_value = serviceType });
            parameters.Add(new AppCode.Pro_Parameters { _par_name = "P_EMP_CODE", _par_value = empCode });
            var manager = new AppCode.Pro_Parameters { _par_name = "P_MANAGER_CODE", _par_value = "", _par_in_out = "OUT", _par_data_type = "Varchar2" };
            parameters.Add(manager);
            var managerName = new AppCode.Pro_Parameters { _par_name = "P_MANAGER_NAME", _par_value = "", _par_in_out = "OUT", _par_data_type = "Varchar2" };
            parameters.Add(managerName);
            var approvalGroup = new AppCode.Pro_Parameters { _par_name = "P_GROUP_NAME", _par_value = "", _par_in_out = "OUT", _par_data_type = "Varchar2" };
            parameters.Add(approvalGroup);

            AppCode.dbaccess db = new dbaccess();
            db.executeProcedure("ESS_GET_FIRST_LEVEL_MANAGER", parameters);

            var apporvedBy = "";
            if (approvalGroup._par_value != null && !string.IsNullOrEmpty(approvalGroup._par_value.ToString()))
            {
                if (managerName._par_value != null && !string.IsNullOrEmpty(managerName._par_value.ToString()))//!string.IsNullOrEmpty(ESSUserDetail.GetESSUserDetailInstance.ManagerEmpCode))
                {
                    apporvedBy = managerName._par_value.ToString();

                }
                else
                {
                    apporvedBy = approvalGroup._par_value.ToString();// + ((managerName._par_value != null) ? "(" + managerName._par_value.ToString() + ")" : "");
                    //hiddenApprovalManagerCode.Value = 

                }

            }

            WriteSuccessMessage("Will be approved by - " + apporvedBy);

            //Response.Write("Saved successfully.");
        }
    
        void RejectReq()
        {


            var reqHead = Request.Form["ReqId"];
            ESSWorkflowHeader ESSwORKFLOW = new ESSWorkflowHeader(reqHead.ToString(), reqHead.ToString());

            var strquery = "SELECT FUN_ESS_APVL_CMNT_STG(" + reqHead + ",'REJECT') FROM DUAL";
            var strcomm = objDB.execute_scalar(strquery);
            if (strcomm == "YES" && Request.Form["approveComment"].Trim() == "")
            {
                 WriteErrorMessage("Error: Enter Comments");
                return;
            }
            var comments = Request.Form["approveComment"];

            // adde by bala on 13-06-2023 , approver cant approve again and again by clicking same approve button
            if (objDB.execute_scalar("SELECT     TAP_PARAVALUE FROM TAS_ATT_PARAM WHERE TAP_PARANAME='ESS_SLA_DAYS'") == "-1")
            {
                var recApproverExistsCnt = objDB.record_found(@"SELECT 
   count(1)
FROM 
    AUTH_WF_HEADER, AUTH_WF_DOC_DETAIL dd, AUTH_WF_APPROVER_LOG
where     
   AWH_ROW_ID =  AWDD_WF_HEADER_ROW_ID
   and AWDD_ROW_ID = AWAL_WF_DET_ROW_ID
   and AWH_CURRENT_CONFIG_ROW_ID = AWDD_ROW_ID
   and awh_unique_id1='" + reqHead + @"'
   and AWAL_EMP_CODE = '" + user.Emp_Code + @"'
   ");
                if (recApproverExistsCnt <= 0)
                {
                    WriteErrorMessage("Error: The workflow is moved to next level.");
                    return;
                }
            }

            //if (essLeaveRequestDA.Apply(SelectedManager, false, userDetail.Login_Company_Code))
            if (ESSwORKFLOW.UpdateWFStatus(false, comments))//essLeaveRequestDA.Approve(nextMngrCode, true, txtAppComments.Text, ERPCurrentUserInfo.GetCurrentUserInfo.LoginCompanyCode))
            {
                WriteSuccessMessage("The record is rejected.");

            }
            else
                WriteErrorMessage("Error: Unable to reject the record.");

        }

   
        void ApproveReq()
        {


            var reqHead = Request.Form["ReqId"];
            ESSWorkflowHeader ESSwORKFLOW = new ESSWorkflowHeader(reqHead.ToString(), reqHead.ToString());

            

            var strappmand = "SELECT TAP_PARAVALUE FROM  TAS_ATT_PARAM  WHERE  TAP_PARANAME='IS_APP_MAND'";
            var strvalueforApp = objDB.execute_scalar(strappmand);

            if (strvalueforApp == "Y")
            {
                WriteErrorMessage("Error: " + ERP.Resources.Essvalid.COMMENTS);
                return;
            }

            //check payroll is locked or not for given days
            var dtLeaveInfo1 = db.execute_query_retun_datatable(@"
                    SELECT 
                        to_char(ELR_FROM_DT,'dd-mm-yyyy') ELR_FROM_DT , to_char(ELR_TO_DT,'dd-mm-yyyy') ELR_TO_DT, ELR_LEAVE_REQ_HEAD_ID
                    FROM ESS_LEAVE_REQ where ELR_LEAVE_REQ_HEAD_ID ='" + reqHead + @"' and ELR_SERVICE_TYPE IN (1,37)
                    ");
            if (dtLeaveInfo1 != null && dtLeaveInfo1.Rows.Count > 0)
            {
                var fDate = dtLeaveInfo1.Rows[0]["ELR_FROM_DT"].ToString().ToArabicDate();
                var tDate = dtLeaveInfo1.Rows[0]["ELR_TO_DT"].ToString().ToArabicDate();
                var cnt = db.execute_scalar(@"Select ESS_PAYROLL_LOCKED_FOR_PERIOD ( to_date('" + fDate.ToString("dd-MM-yyyy") + @"','dd-MM-yyyy') ,  to_date('" + tDate.ToString("dd-MM-yyyy") + @"','dd-MM-yyyy') ) From Dual").ToInt();
                if (cnt > 0)
                {
                    // 
                    WriteErrorMessage("Error: Unable to approve, as payroll is locked for the period.");
                    return;
                }
            }


            var comments = Request.Form["approveComment"];

            var orgCost = "";

            if (comments.Length > 1400)
            {
                WriteErrorMessage("Error: Comments cant exceed 1400 characters.");
                return;
            }
            if (!string.IsNullOrEmpty(Request["EARD_ORG_SOFTWARE_COST"]))
            {
                orgCost = Request["EARD_ORG_SOFTWARE_COST"].ToString();

                objDB.execute_query(@"
UPDATE ESS_AER_REQUEST_DET
SET    
       EARD_ORG_SOFTWARE_COST    = '" + orgCost + @"'
WHERE  EARD_REQUEST_HEAD_ID      = '" + reqHead + @"'
");

            }
            // adde by bala on 13-06-2023 , approver cant approve again and again by clicking same approve button
            // adde by bala on 13-06-2023 , approver cant approve again and again by clicking same approve button
            if (!IsApproverFoundInWorkflow(reqHead.ToInt()))
                return;
            // service
            // ot
            var serviceTypeId = db.execute_scalar(@"SELECT ELR_SERVICE_TYPE FROM ESS_LEAVE_REQ WHERE ELR_LEAVE_REQ_HEAD_ID='"+ reqHead+"'") ;
            if (serviceTypeId=="16")
            {
                var varOTDetail = Request["OTDETAILS"];
                var arr = varOTDetail.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                if (arr.Count() <= 0)
                {
                WriteErrorMessage("Error: Unable to approve the record as n.");
                    return;
                }
                List<string> lststatus = new List<string>();
                foreach (var item in arr)
                {
                    var rowId = item.Split(":".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[0];
                    var appRej = item.Split(":".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[1];
                    
                    string strupdate= "UPDATE ESS_OT_REQ_DTL SET EORD_OT_REQ_DET_STATUS = '"+ appRej+"' WHERE EORD_OT_REQ_DETAIL_ID="+ rowId+"";
                    log_error.write_to_log_file("otdetails", "false", strupdate);
                    lststatus.Add(strupdate);
                }
                bool status = objDB.executeQueryInTrasnaction(lststatus);
            }
            //if (essLeaveRequestDA.Apply(SelectedManager, false, userDetail.Login_Company_Code))
            if (ESSwORKFLOW.UpdateWFStatus(true, comments))//essLeaveRequestDA.Approve(nextMngrCode, true, txtAppComments.Text, ERPCurrentUserInfo.GetCurrentUserInfo.LoginCompanyCode))
            {
                var leaveReq = db.execute_query_retun_datatable(@"select to_char(ELR_FROM_DT, 'dd-mm-yyyy') ELR_FROM_DT,to_char(ELR_TO_DT, 'dd-mm-yyyy')  ELR_TO_DT, ELR_LEAVE_REQ_HEAD_ID from ess_leave_Req where ELR_REQ_STATUS_ID != 3 and ELR_SERVICE_TYPE=1 and ELR_LEAVE_REQ_HEAD_ID = " + reqHead);
                if (leaveReq != null && leaveReq.Rows.Count > 0)
                {
                    CallAPI(leaveReq.Rows[0]["ELR_FROM_DT"].ToString().ToArabicDate(), leaveReq.Rows[0]["ELR_TO_DT"].ToString().ToArabicDate());
                }
                WriteSuccessMessage("The record is approved.");
            }
            else
                WriteErrorMessage("Error: Unable to approve the record.");



            //ess.ELR_LEAVE_ID = "";
            //ess.ELR_FROM_DT = Request.Form["ESSfromDate"].ToArabicDate();
            //ess.ELR_TO_DT = Request.Form["ESStoDate"].ToArabicDate();
            //ess.AttendacneOtherReasonDesc = ess.ELR_SUBJ = ess.ELR_BODY = Request.Form["leavedesc"];
            //ess.AttendacneReasonCode = Request.Form["ddlLeaveType"];


            //Response.Write("Saved successfully.");
        }
        void CallAPI(DateTime dtFro, DateTime dtTo)
        {

            if (string.IsNullOrEmpty(ConfigurationManager.AppSettings["ONTCValidationUrl"]))
            {
                LogError.WriteToLogFile("", "", "Unabl;e to call ontc tas valiation ");

                return;
            }
            System.Net.WebClient client = new System.Net.WebClient();
            client.Headers.Add("content-type", "application/json");//set your header here, you can add multiple headers
            string s = Encoding.ASCII.GetString(client.UploadData(ConfigurationManager.AppSettings["ONTCValidationUrl"] + "/ValidatePunch?fromDate=" +
                dtFro.ToString("yyyy-MM-dd") + @"&toDate=" + dtTo.ToString("yyyy-MM-dd"),
                "GET", null));
            LogError.WriteToLogFile("", "", "ONTC TAS Validation by service result " + s);




        }

        void ApproveLeaveCancel()
        {
            List<string> quries = new List<string>();

            var reqHead = Request.Form["ReqId"];
            ESSWorkflowHeader ESSwORKFLOW = new ESSWorkflowHeader(reqHead.ToString(), reqHead.ToString());

            var strquery = "SELECT FUN_ESS_APVL_CMNT_STG(" + reqHead + ",'APPROVE') FROM DUAL";
            var strcomm = objDB.execute_scalar(strquery);
            if (strcomm == "YES" && Request.Form["approveComment"].Trim() == "")
            {
                WriteErrorMessage("Error: Enter Comments.");
                return;
            }
            var comments = Request.Form["approveComment"];

            var orgCost = "";

            if (comments.Length > 1400)
            {
                WriteErrorMessage("Error: Comments cant exceed 1400 characters.");
                return;
            }
            if (!string.IsNullOrEmpty(Request["EARD_ORG_SOFTWARE_COST"]))
            {
                orgCost = Request["EARD_ORG_SOFTWARE_COST"].ToString();

                objDB.execute_query(@"
UPDATE ESS_AER_REQUEST_DET
SET    
       EARD_ORG_SOFTWARE_COST    = '" + orgCost + @"'
WHERE  EARD_REQUEST_HEAD_ID      = '" + reqHead + @"'
");

            }
            // adde by bala on 13-06-2023 , approver cant approve again and again by clicking same approve button
            if (!IsApproverFoundInWorkflow(reqHead.ToInt()))
                return;
            //if (essLeaveRequestDA.Apply(SelectedManager, false, userDetail.Login_Company_Code))
            if (ESSwORKFLOW.UpdateWFStatus(true, comments))//essLeaveRequestDA.Approve(nextMngrCode, true, txtAppComments.Text, ERPCurrentUserInfo.GetCurrentUserInfo.LoginCompanyCode))
            {
                var Refid = db.execute_scalar("select ELR_LOAN_REFERENCE from ESS_LEAVE_REQ WHERE ELR_SERVICE_TYPE=28 AND ELR_REQ_STATUS_ID IN(7,8) AND ELR_LEAVE_REQ_HEAD_ID=" + reqHead + "");
                if (!string.IsNullOrEmpty(Refid))
                {
                    quries.Add(@"UPDATE ESS_LEAVE_REQ_HEAD SET ELRH_REQ_STATUS_ID =11 WHERE ELRH_LEAVE_REQ_ID   =" + Refid + "");
                    quries.Add(@"UPDATE ESS_LEAVE_REQ SET ELR_REQ_STATUS_ID =11 WHERE ELR_LEAVE_REQ_HEAD_ID   =" + Refid + "");

                    db.executeQueryInTrasnaction(quries);
                }

                WriteSuccessMessage("The record is approved.");
            }
            else
                WriteErrorMessage("Error: Unable to approve the record.");

        }

        void SaveTECH()
        {


            essLeaveRequestDA = new EssLeaveRequestDA();
            essLeaveRequestHeader.LeaveReqDetails.Add(new EssLeaveRequest
            {
                ELR_LEAVE_REQ_ID = (essLeaveRequestHeader.LeaveReqDetails.Count() > 0 ? essLeaveRequestHeader.LeaveReqDetails.Max(m => m.ELR_LEAVE_REQ_ID) + 1 : 1),
                ELR_SERVICE_TYPE = ESSServices.Leave
            });

            EssLeaveRequest ess = essLeaveRequestHeader.LeaveReqDetails[0];

       
            ess.ELR_LEAVE_ID = "";
            ess.ELR_FROM_DT = Request.Form["ESSfromDate"].ToArabicDate();
            ess.ELR_TO_DT = Request.Form["ESSfromDate"].ToArabicDate();
            ess.ELR_NOTIFY_TO = Request.Form["empActingHidden"];

            ess.ReqServiceTypeCode = Request.Form["ddlLeaveType"];
            ess.ELR_REQUESTED_DT = Request.Form["ESSfromDate"].ToArabicDate();
            ess.ELR_EMP_ID = Request.Form["EMPCode"];

            var From_Dt = Request.Form["ESSfromDate"].ToArabicDate().ToString("dd/MM/yyyy");

            var statusOfWF = APPROVER_DETAIL_SERVICE(ess.ELR_EMP_ID, ess.ReqServiceTypeCode.ToInt());

            if (!string.IsNullOrEmpty(statusOfWF))
            {
                ResponseWrite("Error: " + statusOfWF);
                return;
            }
            var totLeaveCnt = db.execute_scalar(@"
SELECT COUNT(*) FROM ESS_LEAVE_REQ ,
        (SELECT TO_DATE( '" + From_Dt + @"', 'dd/MM/yyyy')  A ,  TO_DATE('" + From_Dt + @"' , 'dd/MM/yyyy')  B FROM DUAL) X
WHERE ELR_SERVICE_TYPE = 1
  AND  ELR_EMP_ID='" + ess.ELR_EMP_ID + @"' 
    AND ((ELR_FROM_DT BETWEEN X.A AND X.B OR
        ELR_TO_DT BETWEEN X.A AND X.B) OR 
          ( ELR_FROM_DT < X.A AND X.B < ELR_TO_DT ))
        AND ELR_REQ_STATUS_ID NOT IN ('1','5','6', '11')").ToInt();
            if (totLeaveCnt > 0)
            {

                WriteErrorMessage("Error: Leave applied on requested date.");
                return;
            }

            if (ess.ReqServiceTypeCode == "6")//folder access
            {
                ess.EARD_FOLDER_NAME = Request["EARD_FOLDER_NAME"].ToString();
                ess.EARD_PURPOSE = Request["EARD_PURPOSE"].ToString();

                ess.ELR_BODY = ess.ReqServiceDesc = "Folder access";
                essLeaveRequestHeader.ELR_BODY = "Folder access";

                // validate
                if (ess.EARD_PURPOSE.Length > 498)
                {
                    WriteErrorMessage("Error: Purpose field exceeded the max allowed length 500.");
                    return;
                }


            }
            else if (ess.ReqServiceTypeCode == "7")//it equipment
            {
                ess.EARD_EQUIP_TYPE = Request["EARD_EQUIP_TYPE"].ToString();
                ess.EARD_PURPOSE = Request["EQUIP_PURPOSE"].ToString();
                ess.EARD_EQUIP_OTHER = Request["EARD_EQUIP_OTHER"].ToString();

                essLeaveRequestHeader.ELR_BODY = ess.ELR_BODY = ess.ReqServiceDesc = "IT Equipment Request";

                if (ess.EARD_PURPOSE.Length > 498)
                {
                    WriteErrorMessage("Error: Purpose field exceeded the max allowed length 500.");
                    return;
                }
            }
            else if (ess.ReqServiceTypeCode == "8")//email request
            {
                ess.EARD_EMAIL_TYPE_OF_USE = Request["EARD_EMAIL_TYPE_OF_USE"].ToString();
                ess.EARD_EMAIL_GROUP_MEMBERS = Request["EARD_EMAIL_GROUP_MEMBERS"].ToString();
                ess.EARD_GROUP_EMAIL_NAME = Request["EARD_GROUP_EMAIL_NAME"].ToString();
                ess.EARD_INDIVIDUAL_NAME = Request["EARD_INDIVIDUAL_NAME"].ToString();
                ess.EARD_EMAIL_DURATION_USE = Request["EARD_EMAIL_DURATION_USE"].ToString();
                ess.EARD_EMAIL_FROM = Request["EARD_EMAIL_FROM"].ToString().ToArabicDateNull();
                ess.EARD_EMALI_TO = Request["EARD_EMALI_TO"].ToString().ToArabicDateNull();
                //EARD_PURPOSE1
                ess.EARD_PURPOSE = Request["EARD_PURPOSE1"].ToString();//.ToArabicDateNull();
                if (ess.EARD_EMAIL_DURATION_USE == "L" && ess.EARD_EMALI_TO.HasValue && ess.EARD_EMAIL_FROM.HasValue)
                {
                    if (ess.EARD_EMAIL_FROM.Value > ess.EARD_EMALI_TO.Value)
                    {
                        //11 > 12 will not work
                        //12 > 11 work 
                        WriteErrorMessage("Error: From date should be less than to date.");
                        return;
                    }
                }
                if (ess.EARD_PURPOSE.Length > 499)
                {
                    WriteErrorMessage("Error: Purpose field should be less than 500 characters length.");
                    return;
                }
                essLeaveRequestHeader.ELR_BODY = ess.ELR_BODY = ess.ReqServiceDesc = "Email Request";
            }
            else if (ess.ReqServiceTypeCode == "9")//news
            {
                ess.EARD_NEWS_TITLE = Request["EARD_NEWS_TITLE"].ToString();
                ess.EARD_NEWS_BODY = Request["EARD_NEWS_BODY"].ToString();
                ess.EARD_NEWS_BODY_AR = Request["EARD_NEWS_BODY_AR"].ToString();
                ess.EARD_NEWS_TITLE_AR = Request["EARD_NEWS_TITLE_AR"].ToString();

                if (ess.EARD_NEWS_BODY.Length > 990)
                {
                    WriteErrorMessage("Error: News field should be less than 1000 characters length.");
                    return;
                }
                if (ess.EARD_NEWS_BODY_AR.Length > 990)
                {
                    WriteErrorMessage("Error: News arabic field should be less than 1000 characters length.");
                    return;
                }

                essLeaveRequestHeader.ELR_BODY = ess.ELR_BODY = ess.ReqServiceDesc = "News";
            }
            else if (ess.ReqServiceTypeCode == "10")//content amend
            {
                ess.EARD_CONTENT_LINK = Request["EARD_CONTENT_LINK"].ToString();
                ess.EARD_CONTENT_ORIG_CONT = Request["EARD_CONTENT_ORIG_CONT"].ToString();
                ess.EARD_CONTENT_SUGG_CONT = Request["EARD_CONTENT_SUGG_CONT"].ToString();
                ess.EARD_CONTENT_ORIG_CONT_AR = Request["EARD_CONTENT_ORIG_CONT_AR"].ToString();
                ess.EARD_CONTENT_SUGG_CONT_AR = Request["EARD_CONTENT_SUGG_CONT_AR"].ToString();

                essLeaveRequestHeader.ELR_BODY = ess.ELR_BODY = ess.ReqServiceDesc = "Content Amend";
                if (ess.EARD_CONTENT_ORIG_CONT.Length > 990)
                {
                    WriteErrorMessage("Error: Original Content field should be less than 1000 characters length.");
                    return;
                }
                if (ess.EARD_CONTENT_SUGG_CONT.Length > 990)
                {
                    WriteErrorMessage("Error: Suggested content field should be less than 1000 characters length.");
                    return;
                }

                if (ess.EARD_CONTENT_ORIG_CONT_AR.Length > 990)
                {
                    WriteErrorMessage("Error: Original Content Ar field should be less than 1000 characters length.");
                    return;
                }
                if (ess.EARD_CONTENT_SUGG_CONT_AR.Length > 990)
                {
                    WriteErrorMessage("Error: Suggested content Ar field should be less than 1000 characters length.");
                    return;
                }
            }
            else if (ess.ReqServiceTypeCode == "11")//new page
            {
                ess.EARD_NEWS_TITLE = Request["EARD_NEWS_TITLE"].ToString();
                ess.EARD_NEWS_BODY = Request["EARD_NEWS_BODY"].ToString();
                ess.EARD_NEWS_BODY_AR = Request["EARD_NEWS_BODY_AR"].ToString();
                ess.EARD_NEWS_TITLE_AR = Request["EARD_NEWS_TITLE_AR"].ToString();

                essLeaveRequestHeader.ELR_BODY = ess.ELR_BODY = ess.ReqServiceDesc = "New Page Request";
                if (ess.EARD_NEWS_BODY.Length > 990)
                {
                    WriteErrorMessage("Error: News field should be less than 1000 characters length.");
                    return;
                }

                if (ess.EARD_NEWS_BODY_AR.Length > 990)
                {
                    WriteErrorMessage("Error: News Ar field should be less than 1000 characters length.");
                    return;
                }

            }
            else if (ess.ReqServiceTypeCode == "12")//files
            {
                ess.EARD_NEWS_TITLE = Request["file_title"].ToString();
                essLeaveRequestHeader.ELR_BODY = ess.ELR_BODY = ess.ReqServiceDesc = "Files";
            }
            else if (ess.ReqServiceTypeCode == "13")//software
            {
                ess.EARD_SOFTWARE_COST = Request["EARD_SOFTWARE_COST"].ToString();
                if (!string.IsNullOrEmpty(Request["EARD_ORG_SOFTWARE_COST"]))
                    ess.EARD_ORG_SOFTWARE_COST = Request["EARD_ORG_SOFTWARE_COST"].ToString();

                if (!string.IsNullOrEmpty(Request["EARD_SOFTEARE_JUSTIFICATION"]))
                    ess.EARD_SOFTEARE_JUSTIFICATION = Request["EARD_SOFTEARE_JUSTIFICATION"].ToString();


                essLeaveRequestHeader.ELR_BODY = ess.ELR_BODY = ess.ReqServiceDesc = "Software";


            }
            else if (ess.ReqServiceTypeCode == "15" || ess.ReqServiceTypeCode == "16")//it equipment
            {

                ess.EARD_PURPOSE = ess.ReqServiceTypeCode == "15" ? "Hardware Issue" : "Software Issue";

                ess.EARD_SOFTEARE_JUSTIFICATION = Request["EARD_SOFTEARE_JUSTIFICATION"].ToString();
                essLeaveRequestHeader.ELR_BODY = ess.ELR_SUBJ = ess.ELR_BODY = ess.ReqServiceDesc = ess.EARD_PURPOSE;

                if (ess.ReqServiceDesc.Length > 498)
                {
                    WriteErrorMessage("Error: Description field exceeded the max allowed length 500.");
                    return;
                }
            }
            else
            {
                ess.ELR_BODY = ess.ReqServiceDesc = Request.Form["leavedesc"];
                essLeaveRequestHeader.ELR_BODY = ess.ELR_BODY;
            }

            ess.ELR_REQUESTED_DT = Request.Form["ESSfromDate"].ToArabicDate();
            ess.ELR_EMP_ID = Request.Form["EMPCode"];

            if (ess.ReqServiceTypeCode == "3" || ess.ReqServiceTypeCode == "4" || ess.ReqServiceTypeCode == "5")
            {


                // validate lengh
                if (ess.ELR_BODY.Length > 490)
                {

                    WriteErrorMessage("Error: Description characters exceeded max allowed length 500.");
                    return;
                }
                // check for atleast one item in stationary request
                if (ess.ReqServiceTypeCode == "4")//stationery
                {
                    bool singleItemEntered = false;
                    for (int i = 0; i < 100; i++)
                    {
                        var statineryId = Request.Form["statineryId" + i];
                        var statineryQty = Request.Form["statineryQty" + i];

                        if (string.IsNullOrEmpty(statineryId))
                        {
                            continue;
                        }
                        if (!string.IsNullOrEmpty(statineryQty))
                        {

                            if (statineryQty.ToDouble() > 0)
                            {

                                singleItemEntered = true;
                            }
                        }

                    }
                    if (!singleItemEntered)
                    {
                        WriteErrorMessage("Error: Enter atleast one item.");
                        return;
                    }
                }


            }


            if (ess.ReqServiceTypeCode == "4")//stationery
            {
                //STORE OTHERS
                //ELR_TRAIN_GOAL
                ess.TrainingGoal = Request.Form["txtOthers"];//.ToString();

            }

            essLeaveRequestHeader.ELR_FROM_DT = ess.ELR_FROM_DT;
            essLeaveRequestHeader.ELR_TO_DT = ess.ELR_TO_DT;
            essLeaveRequestHeader.ELR_LEAVE_ID = ess.ELR_LEAVE_ID;
            essLeaveRequestHeader.ELR_SUBJ = ess.ELR_SUBJ;


            essLeaveRequestHeader.ELR_EMP_ID = ess.ELR_EMP_ID;


            essLeaveRequestHeader.ELR_SERVICE_TYPE = ESSServices.RequestForServices;
            ess.ELR_SERVICE_TYPE = ESSServices.RequestForServices;

            essLeaveRequestHeader.ELR_CREATED_USER_ID = Request.Form["UserId"];

            List<string> quries = new List<string>();
            if (ess.ReqServiceTypeCode == "5")//Vehicles
            {

                //                for (int i = 0; i < 100; i++)
                //                {
                //                    var statineryId = Request.Form["statineryId" + i];
                //                    if (string.IsNullOrEmpty(statineryId))
                //                    {
                //                        continue;
                //                    }
                //ELR_VEH_PURPOSE, ELR_VEH_OPERATOR, ELR_VEH_STAFF, ELR_VEH_NEEDED_ON, ELR_VEH_RETURN_ON
                ess.ELR_VEH_PURPOSE = Request["leavedesc"].ToString();
                ess.ELR_VEH_OPERATOR = Request["ELR_VEH_OPERATOR"].ToString();
                ess.ELR_VEH_STAFF = Request["ELR_VEH_STAFF"].ToString();
                ess.ELR_VEH_RETURN_ON = Request["ELR_VEH_RETURN_ON"].ToString().ToArabicDateNull();
                ess.ELR_VEH_NEEDED_ON = Request["ESSfromDate"].ToString().ToArabicDateNull();

                if (ess.ELR_VEH_NEEDED_ON.Value > ess.ELR_VEH_RETURN_ON.Value)
                {
                    WriteErrorMessage("Error: Vehicle return date should be greater than required date.");
                    return;
                }

                // validate requeired fileds
                if (string.IsNullOrEmpty(ess.ELR_VEH_OPERATOR))
                {
                    WriteErrorMessage("Error: Enter operator.");
                    return;
                }

                if (ess.ELR_VEH_OPERATOR.Length > 100)
                {
                    WriteErrorMessage("Error: Vechicle Operator exceed max length 100.");
                    return;
                }

                if (string.IsNullOrEmpty(ess.ELR_VEH_STAFF))
                {
                    WriteErrorMessage("Error: Enter staff travelling.");
                    return;
                }

                if (ess.ELR_VEH_STAFF.Length > 100)
                {
                    WriteErrorMessage("Error:staff travelling  exceed max length 100.");
                    return;
                }

                if (ess.ELR_SUBJ.Length > 100)
                {
                    WriteErrorMessage("Error:Description  exceed max length 100.");
                    return;
                }

                if (!ess.ELR_VEH_RETURN_ON.HasValue)
                {
                    WriteErrorMessage("Error: Enter return date.");
                    return;
                }


              

            }

            if (string.IsNullOrEmpty(ess.ReqServiceTypeCode))
            {
                WriteErrorMessage("Error: Select the service");
                return;
            }
            if (string.IsNullOrEmpty(ess.ReqServiceTypeCode))
            {
                WriteErrorMessage("Error: Enter the service/item required date.");
                return;
            }

            if (ESSHelperFunctions.IsServiceAlreadyRequested(ess.ELR_FROM_DT.Value.ToString("dd/MM/yyyy"), ess.ELR_TO_DT.Value.ToString("dd/MM/yyyy"), ess.ReqServiceTypeCode.ToIntNull(), ess.ELR_EMP_ID) > 0)
            {
                WriteErrorMessage("Error: Service Type is already requested on the selected days.");
                return;
            }
            /*oldFolderPath = HttpContext.Current.Request.MapPath("~/ESS/Docs/") + "/" + leaveReqHeader.ELR_EMP_ID + "/"
               + "/" + (int)leaveReqHeader.ELR_SERVICE_TYPE + "/" + HttpContext.Current.Session.SessionID;*/

            if (essLeaveRequestDA.Apply("", false, "01"))
            {

                if (ess.ReqServiceTypeCode == "4")//stationery
                {
                    for (int i = 0; i < 100; i++)
                    {
                        var statineryId = Request.Form["statineryId" + i];
                        var statineryQty = Request.Form["statineryQty" + i];

                        if (string.IsNullOrEmpty(statineryId))
                        {
                            continue;
                        }

                        quries.Add(@"
INSERT INTO ESS_STATIONERY_REQ (
   ESR_REQ_HEAD_ID, ESR_ITEM_TYPE, ESR_QUANTITY) 
VALUES ( '" + essLeaveRequestDA.LeaveReqHeaderID + @"',
 '" + statineryId + @"',
 '" + statineryQty.ToDouble() + @"' )
");
                    }

                }

                db.executeQueryInTrasnaction(quries);

                WriteSuccessMessage("The request is sent.");
                //ks.showAlert("alert('The request is sent.'); window.location='" + ResolveUrl(Request.RawUrl) + @"';");
            }
            else
            {
                WriteErrorMessage("Error: Unable to send the request.");
            }


            //Response.Write("Saved successfully.");
        }

        void GeneralRequestApprove()
        {


            var reqHead = Request.Form["ReqId"];
            ESSWorkflowHeader ESSwORKFLOW = new ESSWorkflowHeader(reqHead.ToString(), reqHead.ToString());

            var strquery = "SELECT FUN_ESS_APVL_CMNT_STG(" + reqHead + ",'APPROVE') FROM DUAL";
            var strcomm = objDB.execute_scalar(strquery);
            if (strcomm == "YES" && Request.Form["approveComment"].Trim() == "")
            {
                WriteErrorMessage("Error: Enter Comments.");
                return;
            }
            var comments = Request.Form["approveComment"];

            var orgCost = "";

            if (comments.Length > 1400)
            {
                WriteErrorMessage("Error: Comments cant exceed 1400 characters.");
                return;
            }
            if (!string.IsNullOrEmpty(Request["EARD_ORG_SOFTWARE_COST"]))
            {
                orgCost = Request["EARD_ORG_SOFTWARE_COST"].ToString();

                objDB.execute_query(@"
UPDATE ESS_AER_REQUEST_DET
SET    
       EARD_ORG_SOFTWARE_COST    = '" + orgCost + @"'
WHERE  EARD_REQUEST_HEAD_ID      = '" + reqHead + @"'
");

            }
            // adde by bala on 13-06-2023 , approver cant approve again and again by clicking same approve button
            if (!IsApproverFoundInWorkflow(reqHead.ToInt()))
                return;
            if (ESSwORKFLOW.UpdateWFStatus(true, comments))
            {
                WriteSuccessMessage("The record is approved.");
            }
            else
                WriteErrorMessage("Error: Unable to approve the record.");

        }

        void GENERALSaveDocs(string serviceType)
        {
            long leaveReqHeaderID = 0;
            leaveReqHeaderID = essLeaveRequestDA.LeaveReqHeaderID;


            //essLeaveRequestDA.GetNextHeaderId();
            // string oldFolderPath = HttpContext.Current.Request.MapPath("~/ESS/Docs/") + "/" + leaveReqHeader.ELR_EMP_ID + "/"
            //+ "/" + (int)leaveReqHeader.ELR_SERVICE_TYPE + "/" + HttpContext.Current.Session.SessionID;
            try
            {
                Directory.CreateDirectory(Server.MapPath("~/ESS/Docs"));
            }
            catch { }
            try
            {
                Directory.CreateDirectory(Server.MapPath("~/ESS/Docs/" + Request.Form["EMPCode"]));
            }
            catch { }

            try
            {
                Directory.CreateDirectory(Server.MapPath("~/ESS/Docs/" + Request.Form["EMPCode"] + "/" + serviceType));
            }
            catch { }


            try
            {
                Directory.CreateDirectory(Server.MapPath("~/ESS/Docs/" + Request.Form["EMPCode"] + "/" + serviceType + "/" + leaveReqHeaderID));

            }
            catch { }

            HttpPostedFile file = HttpContext.Current.Request.Files["fileInput"];

            try
            {

                if (file != null && file.ContentLength > 0)
                {
                    string fname = Path.GetExtension(file.FileName);

                    file.SaveAs(Server.MapPath("~/ESS/Docs/" + Request.Form["EMPCode"] + "/" + serviceType + "/" + leaveReqHeaderID + "/" + file.FileName));

                    // file.SaveAs(Server.MapPath("~/ESS/Docs/" + Request.Form["EMPCode"] + "/" + serviceType + "/" + Session.SessionID + "/" + file.FileName));
                    //AppCode.LogError.WriteToLogFile(fname, "extension is", fname);
                    //file2.SaveAs(Server.MapPath(Path.Combine("~/App_Data/", fname)));
                }

                //AppCode.LogError.WriteToLogFile("Before save", "", "");


                // file.SaveAs(Server.MapPath("~/ESS/Docs/" + Request.Form["EMPCode"] + "/" + serviceType + "/" + Session.SessionID + "/FileEn" + fname));
            }
            catch (Exception e1)
            {
                AppCode.LogError.WriteToLogFile(e1.ToString(), "", "");
            }
            HttpPostedFile file2 = Request.Files["fileInputArabic"];

            //check file was submitted
            if (file2 != null && file2.ContentLength > 0)
            {
                string fname = Path.GetExtension(file2.FileName);
                file2.SaveAs(Server.MapPath("~/ESS/Docs/" + Request.Form["EMPCode"] + "/" + serviceType + "/" + leaveReqHeaderID + "/" + file2.FileName));
                //file2.SaveAs(Server.MapPath(Path.Combine("~/App_Data/", fname)));
            }




        }




        void GETENCASHLEAVE_REQ(String empcode)
        {


            var dt = objDB.execute_query_retun_datatable(@"
 
            SELECT ROWNUM SNO,
                PLTM_LEAVE_TYPE_DESC ,
                PELD_LEAVE_AVAILED_DAYS,
                PLTM_ENCASH_MIN, 
                PLTM_ENCASH_MAX,
    
                PLTM_LEAVE_TYPE_CODE,  PLTM_IS_LEAVE_IN_HOURS ,
                 nvl(PLTM_SHOWIN_LVBALANCE,'Y') as PLTM_SHOWIN_LVBALANCE,PELD_LEAVE_DAYS
            FROM 
                PPM_LEAVE_TYPE_MASTER, PPM_EMPLOYEE_LEAVE_DETAILS 
            WHERE
                nvl(PLTM_VISIBLE,'N') ='Y' and 
                PLTM_LEAVE_TYPE_CODE = PELD_LEAVE_TYPE and
                 PELD_CODE='" + empcode + @"' AND 
                 PLTM_COMP_CODE   = '01' AND
                 NVL(PLTM_ENCASH_INDICATOR,'N')='Y'
            ORDER BY   PLTM_LEAVE_TYPE_CODE
            ");

            //        AND HRH_DOCUMENT_DATE BETWEEN TO_DATE(to_char('" + from + @"') ,'dd/mm/yyyy') AND
            //TO_DATE(to_char('" + to + @"'), 'dd/mm/yyyy')
            if (dt != null && dt.Rows.Count > 0)
            {
                var JSONresult = JsonConvert.SerializeObject(dt);
                Response.Write(JSONresult);

            }
            else
            {
                Response.Write("{}");
            }



        }

        
        void ANNUALLEAVE_Approve()
        {


            var reqHead = Request.Form["ReqId"];
            ESSWorkflowHeader ESSwORKFLOW = new ESSWorkflowHeader(reqHead.ToString(), reqHead.ToString());

            var strquery = "SELECT FUN_ESS_APVL_CMNT_STG(" + reqHead + ",'APPROVE') FROM DUAL";
            var strcomm = objDB.execute_scalar(strquery);
            if (strcomm == "YES" && Request.Form["approveComment"].Trim() == "")
            {
                WriteErrorMessage("Error: Enter Comments.");
                return;
            }
            var comments = Request.Form["approveComment"];
            var actualdate = Request.QueryString["actdate"];
            var empid = Request.QueryString["empid"];
            var orgCost = "";
            if (comments.Length > 1400)
            {
                WriteErrorMessage("Error: Comments cant exceed 1400 characters.");
                return;
            }
            if (!string.IsNullOrEmpty(Request["EARD_ORG_SOFTWARE_COST"]))
            {
                orgCost = Request["EARD_ORG_SOFTWARE_COST"].ToString();

                objDB.execute_query(@"
                UPDATE ESS_AER_REQUEST_DET
                SET    
                       EARD_ORG_SOFTWARE_COST    = '" + orgCost + @"'
                WHERE  EARD_REQUEST_HEAD_ID      = '" + reqHead + @"'
                ");

            }
            // adde by bala on 13-06-2023 , approver cant approve again and again by clicking same approve button
            if (!IsApproverFoundInWorkflow(reqHead.ToInt()))
                return;
            if (ESSwORKFLOW.UpdateWFStatus(true, comments))
            {
                //var cnt = db.execute_query(@"UPDATE PPT_ANNUAL_LEAVE_ENTRY SET PALE_RESUME_DATE=TO_DATE('" + actualdate + @"','DD/MM/YYYY')   WHERE PALE_EMP_CODE='" + empid + @"'");
                var cnt1 = db.execute_query(@"UPDATE ESS_LEAVE_REQ A   SET A.ELR_VEH_RETURN_ON=(SELECT ELR_VEH_RETURN_ON
               FROM   ESS_LEAVE_REQ
               WHERE   ELR_LEAVE_REQ_HEAD_ID ='" + reqHead + @"')

            WHERE  EXISTS (SELECT 1
               FROM   ESS_LEAVE_REQ
               WHERE A.ELR_LEAVE_REQ_HEAD_ID=ELR_LOAN_REFERENCE AND ELR_LEAVE_REQ_HEAD_ID = '" + reqHead + @"' AND ELR_REQ_STATUS_ID IN(7,8))");

                WriteSuccessMessage("The record is approved.");
            }
            else
                WriteErrorMessage("Error: Unable to approve the record.");

        }

        bool IsApproverFoundInWorkflow(int reqHead)
        {
            // adde by bala on 13-06-2023 , approver cant approve again and again by clicking same approve button
            if (objDB.execute_scalar("SELECT     TAP_PARAVALUE FROM TAS_ATT_PARAM WHERE TAP_PARANAME='ESS_SLA_DAYS'") == "-1")
            {
                var recApproverExistsCnt = objDB.record_found(@"SELECT 
   count(1)
FROM 
    AUTH_WF_HEADER, AUTH_WF_DOC_DETAIL dd, AUTH_WF_APPROVER_LOG
where     
   AWH_ROW_ID =  AWDD_WF_HEADER_ROW_ID
   and AWDD_ROW_ID = AWAL_WF_DET_ROW_ID
   and AWH_CURRENT_CONFIG_ROW_ID = AWDD_ROW_ID
   and awh_unique_id1='" + reqHead + @"'
   and AWAL_EMP_CODE = '" + user.Emp_Code + @"'
   ");
                if (recApproverExistsCnt <= 0)
                {
                    WriteErrorMessage("Error: The workflow is moved to next level.");
                    return false;
                }
            }
            return true;
        }
        void CHECKINOUT_Approve()
        {

            var todate = Request.Form["ESStoDate"].ToString() + ' ' + Request.QueryString["ttime"].ToString();
            var reqHead = Request.Form["ReqId"];
            ESSWorkflowHeader ESSwORKFLOW = new ESSWorkflowHeader(reqHead.ToString(), reqHead.ToString());

            var strquery = "SELECT FUN_ESS_APVL_CMNT_STG(" + reqHead + ",'APPROVE') FROM DUAL";
            var strcomm = objDB.execute_scalar(strquery);
           
            var comments = Request.Form["approveComment"];
            //var actualdate = Request.QueryString["actdate"];
            var empid = Request.QueryString["empid"];
            var orgCost = "";
            if (comments.Length > 1400)
            {
                WriteErrorMessage("Error: Comments cant exceed 1400 characters.");
                return;
            }
            // adde by bala on 13-06-2023 , approver cant approve again and again by clicking same approve button
            if (!IsApproverFoundInWorkflow(reqHead.ToInt()))
                return;


            if (!string.IsNullOrEmpty(Request["EARD_ORG_SOFTWARE_COST"]))
            {
                orgCost = Request["EARD_ORG_SOFTWARE_COST"].ToString();

                objDB.execute_query(@"
                UPDATE ESS_AER_REQUEST_DET
                SET    
                       EARD_ORG_SOFTWARE_COST    = '" + orgCost + @"'
                WHERE  EARD_REQUEST_HEAD_ID      = '" + reqHead + @"'
                ");

            }

            if (ESSwORKFLOW.UpdateWFStatus(true, comments))
            {

                if (todate != "")
                {
                    db.execute_query(@"UPDATE ESS_LEAVE_REQ SET ELR_TO_DT=TO_DATE('" + todate + @"','DD/MM/YYYY HH24:MI')   WHERE ELR_LEAVE_REQ_HEAD_ID='" + reqHead + @"' AND ELR_SERVICE_TYPE=32");
                }
                WriteSuccessMessage("The record is approved.");
            }
            else
                WriteErrorMessage("Error: Unable to approve the record.");

        }
        void ENCASHMENT_Approve()
        {


            var reqHead = Request.Form["ReqId"];
            ESSWorkflowHeader ESSwORKFLOW = new ESSWorkflowHeader(reqHead.ToString(), reqHead.ToString());

            var strquery = "SELECT FUN_ESS_APVL_CMNT_STG(" + reqHead + ",'APPROVE') FROM DUAL";
            var strcomm = objDB.execute_scalar(strquery);
            if (strcomm == "YES" && Request.Form["approveComment"].Trim() == "")
            {
                WriteErrorMessage("Error: Enter Comments.");
                return;
            }
            var comments = Request.Form["approveComment"];

            var orgCost = "";

            if (comments.Length > 1400)
            {
                WriteErrorMessage("Error: Comments cant exceed 1400 characters.");
                return;
            }
            if (!string.IsNullOrEmpty(Request["EARD_ORG_SOFTWARE_COST"]))
            {
                orgCost = Request["EARD_ORG_SOFTWARE_COST"].ToString();

                objDB.execute_query(@"
                UPDATE ESS_AER_REQUEST_DET
                SET    
                       EARD_ORG_SOFTWARE_COST    = '" + orgCost + @"'
                WHERE  EARD_REQUEST_HEAD_ID      = '" + reqHead + @"'
                ");

            }
            strquery = "";
            // adde by bala on 13-06-2023 , approver cant approve again and again by clicking same approve button
            if (!IsApproverFoundInWorkflow(reqHead.ToInt()))
                return;
            if (ESSwORKFLOW.UpdateWFStatus(true, comments))
            {
                strquery = @" INSERT INTO  PPT_ANNUAL_LEAVE_ENTRY (
   PALE_COMP_CODE, PALE_DOC_NO, PALE_DOC_DATE, 
   PALE_EMP_CODE, PALE_LEAVE_TYPE, PALE_START_DATE, 
   PALE_END_DATE, PALE_NO_OF_DAYS, PALE_NO_OF_PAID_DAYS,PALE_IS_FROM_ESS,
    PALE_COMPOFF_GIVEN,PALE_TYPE_ID,PALE_REMARKS) 
 
    SELECT ELR_COMP_CODE1,PAY_ANNUAL_LEAVE_NUMBER.NEXTVAL,TO_DATE(ELRH_REQUESTED_DT,'dd/mm/yyyy')DOC_DATE,PEMP_EMP_CODE,'AL',
    TO_DATE(FROM_DT,'dd/mm/yyyy')FROM_DT,TO_DATE(TO_DT,'dd/mm/yyyy')TO_DT,1,ELRH_TOT_DAYS,'N',ELR_LOAN_AMOUNT,1,LEAVE_BODY
     from ESS_LEAVE_REQ_DETAILS_ALL where  LEAVE_REQ_ID='" + reqHead + @"'";
                objDB.execute_query(strquery);
                WriteSuccessMessage("The record is approved.");
            }
            else
                WriteErrorMessage("Error: Unable to approve the record.");

        }

        void ENCASHMENT_REQ()
        {
            bool isLeaveAttchMandotry = false;
            essLeaveRequestDA = new EssLeaveRequestDA();
            essLeaveRequestHeader.LeaveReqDetails.Add(new EssLeaveRequest
            {
                ELR_LEAVE_REQ_ID = (essLeaveRequestHeader.LeaveReqDetails.Count() > 0 ? essLeaveRequestHeader.LeaveReqDetails.Max(m => m.ELR_LEAVE_REQ_ID) + 1 : 1),
                ELR_SERVICE_TYPE = ESSServices.Encashment
            }); ;

            EssLeaveRequest ess = essLeaveRequestHeader.LeaveReqDetails[0];
            ess.ELR_REQUESTED_DT = Request.Form["ESSfromDate"].ToArabicDate();
            ess.ELR_LEAVE_ID = "AL";
            ess.ELR_FROM_DT = DateTime.Now.Date;
            ess.ELR_TO_DT = DateTime.Now.Date;
            ess.ELR_SUBJ = Request.Form["ddlcreditto"];
            if (Request.Form["leavedesc"].Trim() == "")
                ess.AttendacneOtherReasonDesc = ess.ELR_BODY = "Encashment";
            else
                ess.AttendacneOtherReasonDesc = ess.ELR_BODY = Request.Form["leavedesc"];  // = "Letter Request";

            ess.AttendacneReasonCode = "";
            if (Request.QueryString["AL"] != null)
                ess.ELR_TOT_DAYS = Request.QueryString["AL"].ToDouble();

            if (Request.QueryString["COFF"] != null)
                ess.ELR_LOAN_AMOUNT = Request.QueryString["COFF"].ToDouble();

             
            ess.ELR_EMP_ID = Request.Form["EMPCode"];
            ess.ELR_LETTER_TYPE = "";
            ess.ELR_LETTER_TO_BANK = "";
            //ess.ELR_LETTER_YYYYMM = "";

            essLeaveRequestHeader.ELR_FROM_DT = ess.ELR_FROM_DT;
            essLeaveRequestHeader.ELR_TO_DT = ess.ELR_TO_DT;
            essLeaveRequestHeader.ELR_LEAVE_ID = ess.ELR_LEAVE_ID;
            essLeaveRequestHeader.ELR_SUBJ = ess.ELR_SUBJ;
            essLeaveRequestHeader.ELR_BODY = ess.ELR_BODY;
            essLeaveRequestHeader.ELR_TOT_DAYS = ess.ELR_TOT_DAYS;
            essLeaveRequestHeader.ELR_LOAN_AMOUNT = ess.ELR_LOAN_AMOUNT;
            essLeaveRequestHeader.ELR_EMP_ID = ess.ELR_EMP_ID;
            essLeaveRequestHeader.ELR_REQUESTED_DT = ess.ELR_REQUESTED_DT;


            essLeaveRequestHeader.ELR_SERVICE_TYPE = ESSServices.Encashment;
            ess.ELR_SERVICE_TYPE = ESSServices.Encashment;
            //CountryName

            essLeaveRequestHeader.ELR_CREATED_USER_ID = Request.Form["UserId"];
            ess.ELR_LOAN_REFERENCE = "";


            //.ELR_CREATED_USER_ID = Request.Form["UserId"];
            if (ess.ELR_FROM_DT == null || ess.ELR_TO_DT == null)
            {
                WriteErrorMessage("Error: Enter the from date and to date values");
                return;
            }
            if (ess.ELR_FROM_DT.Value > ess.ELR_TO_DT.Value)
            {
                WriteErrorMessage("Error: From date should be greater than to date.");
                return;
            }
            //ELR_LETTER_TO_BANK, ELR_LETTER_YYYYMM,ELR_LETTER_TYPE
            string strdata = Request.Form["ESSfromDate"];

            var From_Dt = DateTime.Now.ToString("dd/MM/yyyy");
            int totRecordsPending = objDB.record_found(@"
            SELECT COUNT(*) FROM ESS_LEAVE_REQ 
            WHERE ELR_SERVICE_TYPE = 25
              AND  ELR_EMP_ID='" + ess.ELR_EMP_ID + @"' and to_char(ELR_REQUESTED_DT,'dd/mm/yyyy')='" + strdata + @"'
                    AND ELR_REQ_STATUS_ID   IN  ('3','4' )    
            ");
            if (totRecordsPending > 0)
            {
                WriteErrorMessage("Error: Previous  request is pending.");
                return;
            } 
            if (ess.ELR_BODY.Length > 1499)
            {
                Response.Write("Error: Purpose/Remarks Total characters cant be more than 2000.");
                return;
            }
             
            if (isLeaveAttchMandotry)
            {
                var oldFolderPath = HttpContext.Current.Request.MapPath("~/ESS/Docs/") + "/" + ess.ELR_EMP_ID + "/"
               + "/" + (int)ess.ELR_SERVICE_TYPE + "/" + HttpContext.Current.Session.SessionID;
                var filesAttached = false;
                if (Directory.Exists(oldFolderPath))
                {
                    HttpPostedFile file = HttpContext.Current.Request.Files["fileInput"];

                    var files = Directory.GetFiles(oldFolderPath);

                    if (file != null && file.ContentLength > 0)
                    {
                        filesAttached = true;
                    }
                }

                if (filesAttached == false)
                {
                    WriteErrorMessage("Error: attachment is mandatory.");
                    return;
                }
            }
            if (essLeaveRequestDA.Apply("", false, "01"))
            {
                GENERALSaveDocs("25");
                WriteSuccessMessage("The request is sent.");

            }
            else
            {
                WriteErrorMessage("Unable to send the request.");
            }
        }


        void LOAD_ENCASHMENT_HISTORY(string empCode)
        {
            var Services = Request.Form["Service"];
            var fromDate1 = Request.QueryString["FromDate"].ToArabicDate(); ;
            var toDate1 = Request.QueryString["ToDate"].ToArabicDate();

            string strquery = @"SELECT 
     ESS_LEAVE_REQ_DETAILS_ALL.*,
     TO_CHAR(ESS_LEAVE_REQ_DETAILS_ALL.ELRH_REQUESTED_DT,'DD-Mon-YYYY','nls_date_language=english') DT1,       
          APPROVED_GROUP_NAME 
          CURR_STATUS_NAME_1,to_char(ELRH_REQUESTED_DT,'dd/mm/yyyy') REQUESTED_DT,APP_CNT,
 CONCAT(CONCAT(APP_DATE,' '),APP_TIME)APPROVEDDATE,APP_TIME,ELRH_REQ_STATUS_ID
   
FROM 
    ESS_LEAVE_REQ_DETAILS_ALL,
        ESS_REQ_APPROVED_GROUP,
      ( select AWDD_WF_HEADER_ROW_ID WF_HEADER_ID, SUM(CASE WHEN wd2.AWDD_STATUS in ('A','R') THEN 1 ELSE 0 END) AS APP_CNT
  from AUTH_WF_DOC_DETAIL wd2  
                    GROUP BY AWDD_WF_HEADER_ROW_ID)
    where
             UNIQUE_ID_1 =  TO_CHAR(LEAVE_REQ_ID)
                    AND UNIQUE_ID_2 = TO_CHAR(LEAVE_REQ_ID) 
    and WF_HEADER_ID(+) = AWH_ROW_ID1
    AND EST_SERVICE_ID =25
            AND EMP_ID = '" + empCode + @"'
             and((FROM_DT BETWEEN to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy') OR

           TO_DT BETWEEN to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy')) OR
               (FROM_DT < to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy') < TO_DT))

ORDER BY LEAVE_REQ_ID DESC";

            // and EST_SERVICE_ID = " + Services + @"

            DataTable dt1 = objDB.execute_query_retun_datatable(strquery);
            if (dt1 != null && dt1.Rows.Count > 0)
            {
                var JSONresult = JsonConvert.SerializeObject(dt1);
                Response.Write(JSONresult);
            }
            else
            {
                Response.Write("{}");
            }


        }

        void LOAD_ENCASHMENT_APPROVED(string empCode)
        {

            var from = Request["ESSfromDate"].ToArabicDate();
            var to = Request["rtpStartTime"].ToArabicDate();
            var Services = Request.Form["Service"];
            DBAccess ObjDb = new DBAccess();
             string strquery = @"

SELECT 
     ESS_LEAVE_REQ_DETAILS_ALL.*,
      
          APPROVED_GROUP_NAME 
          CURR_STATUS_NAME_1,to_char(ELRH_REQUESTED_DT,'dd/mm/yyyy') REQUESTED_DT,
CONCAT(CONCAT(APP_DATE,' '),APP_TIME)APPROVEDDATE,APP_TIME
    
   
FROM 
     ESS_LEAVE_REQ_DETAILS_ALL,
        ESS_REQ_APPROVED_GROUP,
      ( select AWDD_WF_HEADER_ROW_ID WF_HEADER_ID, SUM(CASE WHEN wd2.AWDD_STATUS in ('A','R') THEN 1 ELSE 0 END) AS APP_CNT
  from AUTH_WF_DOC_DETAIL wd2  
                    GROUP BY AWDD_WF_HEADER_ROW_ID)
    where
             UNIQUE_ID_1 =  TO_CHAR(LEAVE_REQ_ID)
                    AND UNIQUE_ID_2 = TO_CHAR(LEAVE_REQ_ID) 
    and WF_HEADER_ID(+) = AWH_ROW_ID1
    AND EST_SERVICE_ID =25      

and exists ( 
    SELECT 1 FROM AUTH_WF_HEADER, AUTH_WF_DOC_DETAIL
              WHERE     
                    AWDD_WF_HEADER_ROW_ID = AWH_ROW_ID
                    AND AWDD_EMP_APPROVED_BY = '" + empCode + @"'
                    AND AWH_UNIQUE_ID1 =  TO_CHAR(LEAVE_REQ_ID)
                    AND AWH_UNIQUE_ID2 = TO_CHAR(LEAVE_REQ_ID)
    )

             and((FROM_DT BETWEEN to_date('" + from.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy') AND to_date('" + to.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy') OR
            TO_DT BETWEEN to_date('" + from.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy') AND to_date('" + to.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy')) OR
                (FROM_DT < to_date('" + from.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy') AND to_date('" + to.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy') < TO_DT))
ORDER BY LEAVE_REQ_ID DESC";

            DataTable dt1 = objDB.execute_query_retun_datatable(strquery);
            if (dt1 != null && dt1.Rows.Count > 0)
            {
                var JSONresult = JsonConvert.SerializeObject(dt1);
                Response.Write(JSONresult);
            }
            else
            {
                Response.Write("{}");
            }
        }



    

        void GENERAL_REQ()
        {
            bool isLeaveAttchMandotry = false;
            essLeaveRequestDA = new EssLeaveRequestDA();
            essLeaveRequestHeader.LeaveReqDetails.Add(new EssLeaveRequest
            {
                ELR_LEAVE_REQ_ID = (essLeaveRequestHeader.LeaveReqDetails.Count() > 0 ? essLeaveRequestHeader.LeaveReqDetails.Max(m => m.ELR_LEAVE_REQ_ID) + 1 : 1),
                ELR_SERVICE_TYPE = ESSServices.GeneralRequest
            });

            EssLeaveRequest ess = essLeaveRequestHeader.LeaveReqDetails[0];
            ess.ELR_REQUESTED_DT = Request.Form["ESSfromDate"].ToArabicDate();
            ess.ELR_LEAVE_ID = "";
            ess.ELR_FROM_DT = DateTime.Now.Date;
            ess.ELR_TO_DT = DateTime.Now.Date;
            ess.ELR_SUBJ = Request.Form["leavesubj"];
            ess.AttendacneOtherReasonDesc = ess.ELR_BODY = Request.Form["leavedesc"]; ; // = "Letter Request";
            ess.AttendacneReasonCode = "";
            ess.ELR_LOAN_REFERENCE = Request.Form["ddlcategory"];

             

            ess.ELR_EMP_ID = Request.Form["EMPCode"];
            ess.ELR_LETTER_TYPE = "";
            ess.ELR_LETTER_TO_BANK = "";
            //ess.ELR_LETTER_YYYYMM = "";

            essLeaveRequestHeader.ELR_FROM_DT = ess.ELR_FROM_DT;
            essLeaveRequestHeader.ELR_TO_DT = ess.ELR_TO_DT;
            essLeaveRequestHeader.ELR_LEAVE_ID = ess.ELR_LEAVE_ID;
            essLeaveRequestHeader.ELR_SUBJ = ess.ELR_SUBJ;
            essLeaveRequestHeader.ELR_BODY = ess.ELR_BODY;

            essLeaveRequestHeader.ELR_EMP_ID = ess.ELR_EMP_ID;


            essLeaveRequestHeader.ELR_SERVICE_TYPE = ESSServices.GeneralRequest;
            ess.ELR_SERVICE_TYPE = ESSServices.GeneralRequest;
            //CountryName

            essLeaveRequestHeader.ELR_CREATED_USER_ID = Request.Form["UserId"];
            //ess.ELR_LOAN_REFERENCE = "";


            //.ELR_CREATED_USER_ID = Request.Form["UserId"];
            if (ess.ELR_FROM_DT == null || ess.ELR_TO_DT == null)
            {
                WriteErrorMessage("Error: Enter the from date and to date values");
                return;
            }
            if (ess.ELR_FROM_DT.Value > ess.ELR_TO_DT.Value)
            {
                WriteErrorMessage("Error: From date should be greater than to date.");
                return;
            }
            //ELR_LETTER_TO_BANK, ELR_LETTER_YYYYMM,ELR_LETTER_TYPE
            var From_Dt = Request.Form["ESSfromDate"];
            int totRecordsPending = objDB.record_found(@"
SELECT COUNT(*) FROM ESS_LEAVE_REQ 
WHERE ELR_SERVICE_TYPE = 23 AND  ELR_LOAN_REFERENCE='" + ess.ELR_LOAN_REFERENCE + @"' AND UPPER(ELR_SUBJ)='" + ess.ELR_SUBJ.ToString().ToUpper() + @"'
  AND  ELR_EMP_ID='" + ess.ELR_EMP_ID + @"' and TO_DATE(ELR_FROM_DT)=TO_DATE('" + From_Dt + @"','DD /MM/YYYY') 
        AND ELR_REQ_STATUS_ID   IN  ('3','4' )    
");
            if (totRecordsPending > 0)
            {
                WriteErrorMessage("Error: already Exist this Date");
                return;
            }

            if (ess.ELR_SUBJ.Length > 499)
            {
                WriteErrorMessage("Error: Subject Total characters cant be more than 500.");
                return;
            }

            if (ess.ELR_BODY.Length > 1499)
            {
                WriteErrorMessage("Error: Purpose/Remarks Total characters cant be more than 2000.");
                return;
            }

            //            var isLeaveAttchMandotry = db.execute_scalar(@" SELECT 
            //     nvl(PLTM_ATTH_MANDATORY,'N') 
            //FROM PPM_LEAVE_TYPE_MASTER  where PLTM_LEAVE_TYPE_CODE='" + ess.ELR_LEAVE_ID + @"' ").Equals("Y");

            //            AppCode.LogError.WriteToLogFile("isLeaveAttchMandotry", false, isLeaveAttchMandotry.ToString());

            if (isLeaveAttchMandotry)
            {
                var oldFolderPath = HttpContext.Current.Request.MapPath("~/ESS/Docs/") + "/" + ess.ELR_EMP_ID + "/"
               + "/" + (int)ess.ELR_SERVICE_TYPE + "/" + HttpContext.Current.Session.SessionID;
                var filesAttached = false;
                if (Directory.Exists(oldFolderPath))
                {
                    HttpPostedFile file = HttpContext.Current.Request.Files["fileInput"];

                    var files = Directory.GetFiles(oldFolderPath);

                    if (file != null && file.ContentLength > 0)
                    {
                        filesAttached = true;
                    }
                }

                if (filesAttached == false)
                {
                    WriteErrorMessage("Error: attachment is mandatory.");
                    return;
                }
            }
            if (essLeaveRequestDA.Apply("", false, "01"))
            {
                GENERALSaveDocs("23");
                WriteSuccessMessage("The request is sent.");

            }
            else
            {
                WriteErrorMessage("Unable to send the request.");
            }
        }

 
        void LOAD_GENERAL_HISTORY(string empCode)
        {
            var Services = Request.Form["Service"];
            var fromDate1 = Request.QueryString["FromDate"].ToArabicDate(); ;
            var toDate1 = Request.QueryString["ToDate"].ToArabicDate();

            string strquery = @"SELECT 
    PEMP_EMP_CODE,PEMP_EMP_NAME,PBM_BRANCH_NAME,PDPM_DEPARTMENT_DESC,PEMP_EMP_DESIGNATION_DESC,ELR_LEAVE_REQ_HEAD_ID,
     TO_CHAR(ELR_REQUESTED_DT,'DD-Mon-YYYY','nls_date_language=english') DT1,       
         CASE WHEN ELR_REQ_STATUS_ID = 11  THEN 
            --'Waiting For ' || Replace(INITCAP(AEG_DESCRIPTION),'Waiting For','') 
            GET_LEAVE_STATUS (ELR_REQ_STATUS_ID)     
        ELSE  
            APPROVED_GROUP_NAME
        END   CURR_STATUS_NAME_1 
          ,to_char(ELR_REQUESTED_DT,'dd/mm/yyyy') REQUESTED_DT,APP_CNT,
          TO_CHAR (ELR_APPROVED_DT, 'DD-MM-YYY hh24:mi') APP_DATE,
            TO_CHAR (ELR_APPROVED_DT, 'hh24:mi')APP_TIME,ELR_REQ_STATUS_ID,          
         MCMD_ENTITY_CODE,UPPER(MCMD_ENTITY_DESC) MCMD_ENTITY_DESC
   
FROM 
      ESS_LEAVE_REQ , MMM_COMMON_MASTERS_DETAIL,
        ESS_REQ_APPROVED_GROUP,V_EMPLOYEE_DETAILS,
      ( select AWDD_WF_HEADER_ROW_ID WF_HEADER_ID, SUM(CASE WHEN wd2.AWDD_STATUS in ('A','R') THEN 1 ELSE 0 END) AS APP_CNT
  from AUTH_WF_DOC_DETAIL wd2  
                    GROUP BY AWDD_WF_HEADER_ROW_ID)
    where
             UNIQUE_ID_1 =  TO_CHAR(ELR_LEAVE_REQ_HEAD_ID) AND ELR_EMP_ID=PEMP_EMP_CODE
                    AND UNIQUE_ID_2 = TO_CHAR(ELR_LEAVE_REQ_HEAD_ID) 
AND ELR_LOAN_REFERENCE=MCMD_ENTITY_CODE AND MCMD_ENTITY_GROUP='GEN_CATEGORY'
      
    and WF_HEADER_ID(+) = AWH_ROW_ID1  
            AND ELR_EMP_ID = '" + empCode + @"'  AND ELR_SERVICE_TYPE =23 
             and((ELR_FROM_DT BETWEEN to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy') OR

           ELR_TO_DT BETWEEN to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy')) OR
               (ELR_FROM_DT < to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy') < ELR_TO_DT))

ORDER BY ELR_LEAVE_REQ_HEAD_ID DESC";

            // and EST_SERVICE_ID = " + Services + @"
            log_error.write_to_log_file("General", "false", strquery);
            DataTable dt1 = objDB.execute_query_retun_datatable(strquery);
            if (dt1 != null && dt1.Rows.Count > 0)
            {
                var JSONresult = JsonConvert.SerializeObject(dt1);
                Response.Write(JSONresult);
            }
            else
            {
                Response.Write("{}");
            }


        }

        void LOAD_GENERAL_APPROVED(string empCode)
        {

            var from = Request["FromDate"].ToArabicDate();
            var to = Request["ToDate"].ToArabicDate();
            var Services = Request.Form["Service"];
            DBAccess ObjDb = new DBAccess();
            // var streval = objDB.execute_scalar("SELECT TAP_PARAVALUE FROM  TAS_ATT_PARAM WHERE  TAP_PARANAME='COMP_OFF_LEAVE_CODE' ");
            string strquery = @"

SELECT 
     PEMP_EMP_CODE,PEMP_EMP_NAME,PBM_BRANCH_NAME,PDPM_DEPARTMENT_DESC,PEMP_EMP_DESIGNATION_DESC,ELR_LEAVE_REQ_HEAD_ID,
     TO_CHAR(ELR_REQUESTED_DT,'DD-Mon-YYYY','nls_date_language=english') DT1,       
          APPROVED_GROUP_NAME 
          CURR_STATUS_NAME_1,to_char(ELR_REQUESTED_DT,'dd/mm/yyyy') REQUESTED_DT,APP_CNT,
          TO_CHAR (ELR_APPROVED_DT, 'DD-MM-YYY hh24:mi') APP_DATE,
            TO_CHAR (ELR_APPROVED_DT, 'hh24:mi')APP_TIME,          
         MCMD_ENTITY_CODE,MCMD_ENTITY_DESC
    
   
FROM 
     ESS_LEAVE_REQ,
        ESS_REQ_APPROVED_GROUP,MMM_COMMON_MASTERS_DETAIL,V_EMPLOYEE_DETAILS,
      ( select AWDD_WF_HEADER_ROW_ID WF_HEADER_ID, SUM(CASE WHEN wd2.AWDD_STATUS in ('A','R') THEN 1 ELSE 0 END) AS APP_CNT
  from AUTH_WF_DOC_DETAIL wd2  
                    GROUP BY AWDD_WF_HEADER_ROW_ID)
    where    ELR_EMP_ID=PEMP_EMP_CODE and
             UNIQUE_ID_1 =  TO_CHAR(ELR_LEAVE_REQ_HEAD_ID)  
                    AND UNIQUE_ID_2 = TO_CHAR(ELR_LEAVE_REQ_HEAD_ID) 
    and WF_HEADER_ID(+) = AWH_ROW_ID1
AND MCMD_ENTITY_CODE=ELR_LOAN_REFERENCE AND MCMD_ENTITY_GROUP='GEN_CATEGORY'
     AND ELR_SERVICE_TYPE =23 

and exists ( 
    SELECT 1 FROM AUTH_WF_HEADER, AUTH_WF_DOC_DETAIL
              WHERE     
                    AWDD_WF_HEADER_ROW_ID = AWH_ROW_ID
                    AND AWDD_EMP_APPROVED_BY = '" + empCode + @"'
                    AND AWH_UNIQUE_ID1 =  TO_CHAR(ELR_LEAVE_REQ_HEAD_ID)
                    AND AWH_UNIQUE_ID2 = TO_CHAR(ELR_LEAVE_REQ_HEAD_ID)
    )

             and((ELR_FROM_DT BETWEEN to_date('" + from.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy') AND to_date('" + to.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy') OR
            ELR_TO_DT BETWEEN to_date('" + from.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy') AND to_date('" + to.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy')) OR
                (ELR_FROM_DT < to_date('" + from.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy') AND to_date('" + to.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy') < ELR_TO_DT))
ORDER BY ELR_LEAVE_REQ_HEAD_ID DESC";

            DataTable dt1 = objDB.execute_query_retun_datatable(strquery);
            if (dt1 != null && dt1.Rows.Count > 0)
            {
                var JSONresult = JsonConvert.SerializeObject(dt1);
                Response.Write(JSONresult);
            }
            else
            {
                Response.Write("{}");
            }
        }


        void LETTER_REQ()
        {


            essLeaveRequestDA = new EssLeaveRequestDA();
            essLeaveRequestHeader.LeaveReqDetails.Add(new EssLeaveRequest
            {
                ELR_LEAVE_REQ_ID = (essLeaveRequestHeader.LeaveReqDetails.Count() > 0 ? essLeaveRequestHeader.LeaveReqDetails.Max(m => m.ELR_LEAVE_REQ_ID) + 1 : 1),
                ELR_SERVICE_TYPE = ESSServices.LetterRequest
            });

            EssLeaveRequest ess = essLeaveRequestHeader.LeaveReqDetails[0];


           

            ess.ELR_LEAVE_ID = "";
            ess.ELR_FROM_DT = DateTime.Now.Date;
            ess.ELR_TO_DT = DateTime.Now.Date;
            ess.AttendacneOtherReasonDesc = ess.ELR_SUBJ = ess.ELR_BODY = Request.QueryString["LetterPurpose"]; ; // = "Letter Request";
            ess.AttendacneReasonCode = Request.Form["ddlLeaveType"];
           
            //toBank,ddlLetter

            var toBank = Request.Form["toBank"];
            var ddlLetter = Request.Form["ddlLetter"];
            var lLettertype = Request.Form["ddlltettertype"];
            ess.ELR_EMP_ID = Request.Form["EMPCode"];
            ess.ELR_LETTER_TYPE = ddlLetter;
            ess.ELR_LETTER_TO_BANK = toBank;

            essLeaveRequestHeader.ELR_FROM_DT = ess.ELR_FROM_DT;
            essLeaveRequestHeader.ELR_TO_DT = ess.ELR_TO_DT;
            essLeaveRequestHeader.ELR_LEAVE_ID = ess.ELR_LEAVE_ID;
            essLeaveRequestHeader.ELR_SUBJ = ess.ELR_SUBJ;
            essLeaveRequestHeader.ELR_BODY = ess.ELR_BODY;

            essLeaveRequestHeader.ELR_EMP_ID = ess.ELR_EMP_ID;

            essLeaveRequestHeader.ELR_LOAN_LOAN_TYPE = ess.ELR_LOAN_LOAN_TYPE;
            essLeaveRequestHeader.ELR_SERVICE_TYPE = ESSServices.LetterRequest;
            ess.ELR_SERVICE_TYPE = ESSServices.LetterRequest;
            //CountryName
            //ELR_LETTER_TO_BANK
            essLeaveRequestHeader.ELR_CREATED_USER_ID = Request.Form["UserId"];
            if(!string.IsNullOrEmpty(Request["AccountNo"]))
            ess.ELR_LOAN_REFERENCE = essLeaveRequestHeader.ELR_LOAN_REFERENCE = Request["AccountNo"];
            else
            essLeaveRequestHeader.ELR_LOAN_REFERENCE = ess.ELR_LOAN_REFERENCE = lLettertype;

            if (ddlLetter == "NOC_LETTER")
            {
                essLeaveRequestHeader.ELR_LETTER_TO_BANK = ess.ELR_LETTER_TO_BANK = Request["CountryName"];

                //CountryName
            }

            if (ddlLetter == "SALARY")
            {
                essLeaveRequestHeader.ELR_LOAN_REFERENCE = ess.ELR_LOAN_REFERENCE = Request["LetterPurpose"];            
            }
            //.ELR_CREATED_USER_ID = Request.Form["UserId"];
            if (ess.ELR_FROM_DT == null || ess.ELR_TO_DT == null)
            {
                WriteErrorMessage("Error: Enter the from date and to date values");
                return;
            }
            if (ess.ELR_FROM_DT.Value > ess.ELR_TO_DT.Value)
            {
                WriteErrorMessage("Error: From date should be greater than to date.");
                return;
            }
            //ELR_LETTER_TO_BANK, ELR_LETTER_YYYYMM,ELR_LETTER_TYPE
            var From_Dt = DateTime.Now.ToString("dd/MM/yyyy");

            int TOTALLOWED = objDB.execute_scalar(@"SELECT 
  NO_OF_TIMES
FROM AMM_LETTERS WHERE AL_LETTER_CODE='" + ddlLetter + @"' ").ToInt();

            int totRecordsPending = objDB.record_found(@"
SELECT COUNT(*) FROM ESS_LEAVE_REQ 
WHERE ELR_SERVICE_TYPE = 13
  AND  ELR_EMP_ID='" + ess.ELR_EMP_ID + @"' 
        AND ELR_REQ_STATUS_ID   IN  (3,8,7 )
    and ELR_LETTER_TYPE = '" + ess.ELR_LETTER_TYPE + @"'
");
            if (TOTALLOWED > 0 && totRecordsPending >= TOTALLOWED)
            {
                WriteErrorMessage("Error: You hae exceeded the maximum number of letter request!.");
                return;
            }

            if (ess.ELR_SUBJ.Length > 499)
            {
                WriteErrorMessage("Error: Total characters cant be more than 500.");
                return;
            }

            if (essLeaveRequestDA.Apply("", false, "01"))
            {
                WriteSuccessMessage("The request is sent.");
                //ks.showAlert("alert('The request is sent.'); window.location='" + ResolveUrl(Request.RawUrl) + @"';", Page);
            }
            else
            {
                WriteErrorMessage("Unable to send the request.");
            }


            //Response.Write("Saved successfully.");
        }

		void CHANGE_BANK_ACCOUNT_REQ()
		{


			essLeaveRequestDA = new EssLeaveRequestDA();
			essLeaveRequestHeader.LeaveReqDetails.Add(new EssLeaveRequest
			{
				ELR_LEAVE_REQ_ID = (essLeaveRequestHeader.LeaveReqDetails.Count() > 0 ? essLeaveRequestHeader.LeaveReqDetails.Max(m => m.ELR_LEAVE_REQ_ID) + 1 : 1),
				ELR_SERVICE_TYPE = ESSServices.ChangeSalaryAccount
			});

			EssLeaveRequest ess = essLeaveRequestHeader.LeaveReqDetails[0];

			ess.ELR_LEAVE_ID = "CHSL";
			ess.ELR_LETTER_TO_BANK = Request["ddltobank"];
			ess.ELR_LOAN_REFERENCE = Request["tobankaccno"];
			ess.ELR_FROM_DT = DateTime.Now.Date;
			ess.ELR_TO_DT = DateTime.Now.Date;
			ess.AttendacneOtherReasonDesc = ess.ELR_SUBJ = ess.ELR_BODY = Request.QueryString["Remarks"]; ; // = "Letter Request";


			ess.ELR_EMP_ID = Request.Form["EMPCode"];

			essLeaveRequestHeader.ELR_FROM_DT = ess.ELR_FROM_DT;
			essLeaveRequestHeader.ELR_TO_DT = ess.ELR_TO_DT;
			essLeaveRequestHeader.ELR_LEAVE_ID = ess.ELR_LEAVE_ID;
			essLeaveRequestHeader.ELR_SUBJ = ess.ELR_SUBJ;
			essLeaveRequestHeader.ELR_BODY = ess.ELR_BODY;

			essLeaveRequestHeader.ELR_EMP_ID = ess.ELR_EMP_ID;


			essLeaveRequestHeader.ELR_SERVICE_TYPE = ESSServices.ChangeSalaryAccount;
			ess.ELR_SERVICE_TYPE = ESSServices.ChangeSalaryAccount;

			essLeaveRequestHeader.ELR_CREATED_USER_ID = Request.Form["UserId"];
			ess.ELR_LOAN_REFERENCE = essLeaveRequestHeader.ELR_LOAN_REFERENCE = Request["tobankaccno"];


			//.ELR_CREATED_USER_ID = Request.Form["UserId"];
			if (ess.ELR_FROM_DT == null || ess.ELR_TO_DT == null)
			{
				WriteErrorMessage("Error: Enter the from date and to date values");
				return;
			}
			if (ess.ELR_FROM_DT.Value > ess.ELR_TO_DT.Value)
			{
				WriteErrorMessage("Error: From date should be greater than to date.");
				return;
			}
			//ELR_LETTER_TO_BANK, ELR_LETTER_YYYYMM,ELR_LETTER_TYPE


			var Sal_Attach = objDB.execute_scalar(@"SELECT NVL(TAP_PARAVALUE,'N')TAP_PARAVALUE FROM TAS_ATT_PARAM WHERE  TAP_PARANAME='IS_ATT_SALACCCHANGE'");
			HttpPostedFile filechksal1 = HttpContext.Current.Request.Files["fileInput"];
			HttpPostedFile filechksal2 = HttpContext.Current.Request.Files["fileInputArabic"];

			if (Sal_Attach == "Y" && filechksal1.ContentLength == 0)
			{
				WriteErrorMessage("Error:NOC Attachment should be required.");
				return;
			}
			if (Sal_Attach == "Y" && filechksal2.ContentLength == 0)
			{
				WriteErrorMessage("Error: Bank Form Attachment should be required.");
				return;
			}

			if (ess.ELR_SUBJ.Length > 499)
			{
				WriteErrorMessage("Error: Remarks can't be more than 500.");
				return;
			}

			if (essLeaveRequestDA.Apply("", false, "01"))
			{
				WriteSuccessMessage("The request is sent.");
				//ks.showAlert("alert('The request is sent.'); window.location='" + ResolveUrl(Request.RawUrl) + @"';", Page);
			}
			else
			{
				WriteErrorMessage("Unable to send the request.");
			}


			//Response.Write("Saved successfully.");
		}

		private void PopulateChangeSalHist()
        {
            var empcode = Request.QueryString["EmpCode"];
            var from = Request["From"].ToArabicDate();
            var to = Request["To"].ToArabicDate();

            var STRQUERY = @"SELECT distinct AWH_ROW_ID1 AWH_ROW_ID,APP_CNT,
               CASE WHEN NVL(APP_CNT,0)=0 THEN 'P' ELSE 'C' END  AS WF_STATUS3,
                ELRH_TOT_DAYS,
                PEMP_EMP_NAME, PEMP_EMP_CODE,
                LEAVE_REQ_ID, to_char(FROM_DT,'dd/mm/yyyy') as FROM_DT, to_char(TO_DT,'dd/mm/yyyy') as TO_DT, 
                SUBJ, LEAVE_BODY, EST_SERVICE_ID, to_char(ELRH_REQUESTED_DT,'dd/mm/yyyy')REQ_DATE,
                EST_SERVICE_DESC, TOBK.FBNM_BANK_NAME,TOBP.FBNB_BRANCH_NAME,
                    CASE WHEN ELRH_REQ_STATUS_ID = 11  THEN 
                        --'Waiting For ' || Replace(INITCAP(AEG_DESCRIPTION),'Waiting For','') 
                        GET_LEAVE_STATUS (ELRH_REQ_STATUS_ID)     
                    ELSE  
                        APPROVED_GROUP_NAME
                    END   CURR_STATUS_NAME, ATT_REASON_DESC1, ELRH_REQ_STATUS_ID,
                ES_CODE, ES_DESC,  
                SERVICE_REQ_DESC, PAYSLIP_REQ_DESC, TRAINING_FEEDBACK_DESC,ELR_LOAN_REFERENCE,
                PLTM_LEAVE_TYPE_CODE, nvl(PLTM_LEAVE_TYPE_DESC, PLTM_LEAVE_ARABIC_DESC) as PLTM_LEAVE_TYPE_DESC,
               STATUS,lr.ELR_LEAVE_REQ_HEAD_ID, ELR_LETTER_TO_BANK, ELR_LETTER_YYYYMM, ELR_LETTER_TYPE,
              TO_CHAR (ELR_APPROVED_DT, 'DD-MM-YYY HH24:MI') APP_DATE,APP_TIME
            FROM 
                ESS_LEAVE_REQ_DETAILS_ALL, 
                ESS_LEAVE_REQ   lr  ,FINM_BANK_MAIN TOBK ,FINM_BANK_BRANCH TOBP ,
                      ESS_REQ_APPROVED_GROUP,
                  ( select AWDD_WF_HEADER_ROW_ID WF_HEADER_ID, SUM(CASE WHEN wd2.AWDD_STATUS in ('A','R') THEN 1 ELSE 0 END) AS APP_CNT
              from AUTH_WF_DOC_DETAIL wd2  
                                GROUP BY AWDD_WF_HEADER_ROW_ID)
                where
             to_char(LEAVE_REQ_ID) = to_char(UNIQUE_ID_1(+))
                   AND to_char(LEAVE_REQ_ID) = to_char(UNIQUE_ID_2  (+))
                   AND EST_SERVICE_ID ='44'
                   and ELR_LEAVE_REQ_HEAD_ID = LEAVE_REQ_ID       
                   and PEMP_EMP_CODE ='" + user.Emp_Code + @"'
   AND ELR_LETTER_TO_BANK  =TOBP.FBNB_BANK_BRANCH(+)
                                  AND TOBK.FBNM_BANK_CODE=TOBP.FBNB_BANK_CODE 
                     and WF_HEADER_ID(+) = AWH_ROW_ID1

            and ((FROM_DT BETWEEN to_date('" + from.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + to.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') OR
                    TO_DT BETWEEN to_date('" + from.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + to.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy')) OR 
                      ( FROM_DT < to_date('" + from.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + to.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') < TO_DT ))
               order by LEAVE_REQ_ID desc
 
            ";
            log_error.write_to_log_file("ChangeSalaryAccount", "false", STRQUERY);
            var dt = db.execute_query_retun_datatable(STRQUERY);
            //AppCode.LogError.WriteToLogFile("", "", q);
            var salhistory = "";

            if (dt != null && dt.Rows.Count > 0)
            {
                if (dt != null && dt.Rows.Count > 0)
                {

                    foreach (DataRow item in dt.Rows)
                    {
                        String DeleteString = @"<td class='green'  > <a href='#' onclick=""DeleteReq('" + item["LEAVE_REQ_ID"].ToString() + @"');"">Cancel </a></td>";

                        if (item["WF_STATUS3"].ToString() == "C")
                        {
                            //WF_STATUS3
                            DeleteString = @"<td class='gray' onclick='alert(""Not allowed."");' > Cancel </td>";
                        }
                        salhistory += @"<tr>
                            <td class='f1'> <a href='javascript:LoadApprover3(" + item["EST_SERVICE_ID"].ToString() + @"," + item["LEAVE_REQ_ID"].ToString() + @")'>" + item["SERVICE_REQ_DESC"].ToString() + @"</a> </td>
                        <td class='gray'> " + item["FROM_DT"].ToString() + @" </td>
                        <td class='green'> " + item["CURR_STATUS_NAME"].ToString() + @" </td>
                        " + DeleteString + @"
                        </tr>

                        ";

                    }

                    //    dt.Columns.Add("FileName"); dt.Columns.Add("FileNameHr");
                    //foreach (DataRow dr in dt.Rows)
                    //{
                    //    if (dr["LEAVE_REQ_ID"].ToString() != "")
                    //    {
                    //        dr["FileName"] = Getfilinfo("44", dr["PEMP_EMP_CODE"].ToString(), dr["LEAVE_REQ_ID"].ToString());
                    //        dr["FileNameHr"] = GetfilinfoHR("44", dr["PEMP_EMP_CODE"].ToString(), dr["LEAVE_REQ_ID"].ToString());
                    //    }

                    //}

                    var JSONresult = JsonConvert.SerializeObject(dt);
                    Response.Write(JSONresult);
                }
                else
                {
                    Response.Write("{}");
                }


            }
			else
			{
                Response.Write("{}");

            }
        }

        private void PopulateChangeSalApproved()
        {
            var empcode= Request["EmpCode"];
            var from = Request["From"].ToArabicDate();
            var to = Request["To"].ToArabicDate();
            //'Waiting for ' || AEG_DESCRIPTION  CURR_STATUS_NAME, 

            var STRQUERY = @"SELECT  
                           CASE WHEN NVL(APP_CNT,0)=0 THEN 'P' ELSE 'C' END  AS WF_STATUS3,
                            ELRH_TOT_DAYS,
                            PEMP_EMP_NAME, PEMP_EMP_CODE,PBM_BRANCH_NAME,
                            LEAVE_REQ_ID, to_char(FROM_DT,'dd/mm/yyyy') as FROM_DT, to_char(TO_DT,'dd/mm/yyyy') as TO_DT, 
                            SUBJ, LEAVE_BODY, EST_SERVICE_ID, ELR_LOAN_REFERENCE,
                            EST_SERVICE_DESC, to_char(ELRH_REQUESTED_DT,'dd/mm/yyyy')REQ_DATE,  SERVICE_REQ_DESC, 
                            TOBK.FBNM_BANK_NAME,TOBP.FBNB_BRANCH_NAME,
                            PLTM_LEAVE_TYPE_CODE, nvl(PLTM_LEAVE_TYPE_DESC, PLTM_LEAVE_ARABIC_DESC) as PLTM_LEAVE_TYPE_DESC,
                               CASE WHEN ELRH_REQ_STATUS_ID = 11  THEN 
                                    --'Waiting For ' || Replace(INITCAP(AEG_DESCRIPTION),'Waiting For','') 
                                    GET_LEAVE_STATUS (ELRH_REQ_STATUS_ID)     
                                ELSE  
                                    APPROVED_GROUP_NAME
                                END   CURR_STATUS_NAME, APP_CNT, CONCAT(CONCAT(APP_DATE,' '),APP_TIME)APP_DATE,APP_TIME
                           FROM 
                            ESS_LEAVE_REQ_DETAILS_ALL, ESS_REQ_APPROVED_GROUP, ESS_LEAVE_REQ   lr,FINM_BANK_MAIN TOBK ,FINM_BANK_BRANCH TOBP ,
                              ( select AWDD_WF_HEADER_ROW_ID WF_HEADER_ID, SUM(CASE WHEN wd2.AWDD_STATUS in ('A','R') THEN 1 ELSE 0 END) AS APP_CNT
                          from AUTH_WF_DOC_DETAIL wd2  
                                            GROUP BY AWDD_WF_HEADER_ROW_ID)
                            where
                                to_char(LEAVE_REQ_ID) =to_char(UNIQUE_ID_1(+))     and ELR_LEAVE_REQ_HEAD_ID = LEAVE_REQ_ID    
                                AND to_char(LEAVE_REQ_ID) = to_char(UNIQUE_ID_2(+))
                            AND EST_SERVICE_ID ='44'                          
                     
                              and WF_HEADER_ID(+) = AWH_ROW_ID1
                         AND ELR_LETTER_TO_BANK  =TOBP.FBNB_BANK_BRANCH(+)
                                  AND TOBK.FBNM_BANK_CODE=TOBP.FBNB_BANK_CODE 
 
                        and ((FROM_DT BETWEEN to_date('" + from.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + to.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') OR
                                TO_DT BETWEEN to_date('" + from.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + to.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy')) OR 
                                  ( FROM_DT < to_date('" + from.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + to.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') < TO_DT ))
 
                         and exists ( 
                            SELECT 1 FROM AUTH_WF_HEADER, AUTH_WF_DOC_DETAIL
                                      WHERE     
                                            AWDD_WF_HEADER_ROW_ID = AWH_ROW_ID
                                            AND AWDD_EMP_APPROVED_BY = '" + user.Emp_Code + @"'
                                            AND AWH_UNIQUE_ID1 =  TO_CHAR(LEAVE_REQ_ID)
                                            AND AWH_UNIQUE_ID2 = TO_CHAR(LEAVE_REQ_ID)
                            )

                         order by LEAVE_REQ_ID desc";



            var dt = db.execute_query_retun_datatable(STRQUERY);

            if (dt != null && dt.Rows.Count > 0)
            {

                var JSONresult = JsonConvert.SerializeObject(dt);
                Response.Write(JSONresult);
            }
            else
            {
                Response.Write("{}");
            }


        }
        void SaveAtt()
        {


            essLeaveRequestDA = new EssLeaveRequestDA();
            essLeaveRequestHeader.LeaveReqDetails.Add(new EssLeaveRequest
            {
                ELR_LEAVE_REQ_ID = (essLeaveRequestHeader.LeaveReqDetails.Count() > 0 ? essLeaveRequestHeader.LeaveReqDetails.Max(m => m.ELR_LEAVE_REQ_ID) + 1 : 1),
                ELR_SERVICE_TYPE = ESSServices.Leave
            });

            EssLeaveRequest ess = essLeaveRequestHeader.LeaveReqDetails[0];

      


            ess.ELR_LEAVE_ID = "";
            ess.ELR_FROM_DT = Request.Form["ESSfromDate"].ToArabicDate();
            ess.ELR_TO_DT = Request.Form["ESStoDate"].ToArabicDate();
            ess.AttendacneOtherReasonDesc = ess.ELR_SUBJ = ess.ELR_BODY = Request.Form["leavedesc"];
            ess.AttendacneReasonCode = Request.Form["ddlLeaveType"];
            ess.ELR_NOTIFY_TO = Request.Form["empActingHidden"];



            ess.ELR_EMP_ID = Request.Form["EMPCode"];

            essLeaveRequestHeader.ELR_FROM_DT = ess.ELR_FROM_DT;
            essLeaveRequestHeader.ELR_TO_DT = ess.ELR_TO_DT;
            essLeaveRequestHeader.ELR_LEAVE_ID = ess.ELR_LEAVE_ID;
            essLeaveRequestHeader.ELR_SUBJ = ess.ELR_SUBJ;
            essLeaveRequestHeader.ELR_BODY = ess.ELR_BODY;

            essLeaveRequestHeader.ELR_EMP_ID = ess.ELR_EMP_ID;


            essLeaveRequestHeader.ELR_SERVICE_TYPE = ESSServices.AttendanceAbsence;
            ess.ELR_SERVICE_TYPE = ESSServices.AttendanceAbsence;

            essLeaveRequestHeader.ELR_CREATED_USER_ID = Request.Form["UserId"];


            if (ess.ELR_FROM_DT == null || ess.ELR_TO_DT == null)
            {
                WriteErrorMessage("Error: Enter the from date and to date values");
                return;
            }
            if (ess.ELR_FROM_DT.Value > ess.ELR_TO_DT.Value)
            {
                WriteErrorMessage("Error: From date should be greater than to date.");
                return;
            }
            var alreaycnt = ESSHelperFunctions.IsLeaveAlreadyApplied(ess.ELR_FROM_DT.Value.ToString("dd/MM/yyyy"), ess.ELR_TO_DT.Value.ToString("dd/MM/yyyy"), null, ess.ELR_EMP_ID);
            if (alreaycnt > 0)
            {
                WriteErrorMessage("Error: You have already applied leave on the selected days.Since you cannot raise attendance request");
                return;
            }

            ess.ELR_TOT_DAYS = essLeaveRequestHeader.ELR_TOT_DAYS = (ess.ELR_TO_DT.Value - ess.ELR_FROM_DT.Value).TotalDays;

            var totDays = Request.Form["txtTotalDays"].ToDouble();
            ess.ELR_TOT_DAYS = essLeaveRequestHeader.ELR_TOT_DAYS = totDays;


            var trueFalse = false;
            if (Request.Form["chkIsFirstDayOff"] != null && Request.Form["chkIsFirstDayOff"] == "on")
            {
                trueFalse = true;
            }
            ess.ELR_START_DAY_HALF = trueFalse;

            trueFalse = false;
            if (Request.Form["chkIsSecondDayOff"] != null && Request.Form["chkIsSecondDayOff"] == "on")
            {
                trueFalse = true;
            }

            ess.ELR_END_DAY_HALF = trueFalse;

            if (totDays <= 0)
            {
                WriteErrorMessage("Error: Total days cant be 0");
                return;
            }
            // CHECK att aleardy applied or not
            var checkQuery = @"
SELECT COUNT(*) FROM ESS_LEAVE_REQ ,
        (SELECT TO_DATE( '" + ess.ELR_FROM_DT.Value.ToString("dd/MM/yyyy") + @"', 'dd/MM/yyyy')  A ,  TO_DATE( '" + ess.ELR_TO_DT.Value.ToString("dd/MM/yyyy") + @"' , 'dd/MM/yyyy')  B FROM DUAL) X
WHERE ELR_SERVICE_TYPE = 5
  AND  ELR_EMP_ID='" + ess.ELR_EMP_ID + @"' 
    AND ((ELR_FROM_DT BETWEEN X.A AND X.B OR
        ELR_TO_DT BETWEEN X.A AND X.B) OR 
          ( ELR_FROM_DT < X.A AND X.B < ELR_TO_DT ))
        AND ELR_REQ_STATUS_ID NOT IN ('1','5','6', '11') ";

            var noOfRecordFoundInAtt = db.execute_scalar(checkQuery).ToInt();
            if (noOfRecordFoundInAtt > 0)
            {
                WriteErrorMessage("Error: The attendance request is already found for the selected days");
                return;
            }

            if (ess.ELR_SUBJ.Length > 499)
            {
                WriteErrorMessage("Error: Total characters cant be more than 500.");
                return;
            }

            if (essLeaveRequestDA.Apply("", false, "01"))
            {
                WriteSuccessMessage("The request is sent.");
            }
            else
            {
                WriteErrorMessage("Unable to send the request.");
            }
        }

          
        

        void SaveLeaveCarryward()
        {

            essLeaveRequestDA = new EssLeaveRequestDA();
            essLeaveRequestHeader.LeaveReqDetails.Add(new EssLeaveRequest
            {
                ELR_LEAVE_REQ_ID = (essLeaveRequestHeader.LeaveReqDetails.Count() > 0 ? essLeaveRequestHeader.LeaveReqDetails.Max(m => m.ELR_LEAVE_REQ_ID) + 1 : 1),
                ELR_SERVICE_TYPE = ESSServices.Leave
            });

            EssLeaveRequest ess = essLeaveRequestHeader.LeaveReqDetails[0];


            ess.ELR_LEAVE_ID = Request.Form["ddlLeaveType"];
            ess.ELR_FROM_DT = DateTime.Now.Date;
            ess.ELR_TO_DT = DateTime.Now.Date;
            ess.ELR_TOT_DAYS = Request.Form["leaveDaysTotal1"].ToDouble();
            var trueFalse = false;
            if (Request.Form["chkIsFirstDayOff"] != null && Request.Form["chkIsFirstDayOff"] == "on")
            {
                trueFalse = true;
            }
            ess.ELR_START_DAY_HALF = trueFalse;

            trueFalse = false;
            if (Request.Form["chkIsSecondDayOff"] != null && Request.Form["chkIsSecondDayOff"] == "on")
            {
                trueFalse = true;
            }

            ess.ELR_END_DAY_HALF = trueFalse;
            ess.ELR_BODY = ess.ELR_SUBJ = Request.Form["leavedesc"];
            if (ess.ELR_SUBJ.Length > 499)
            {
                Response.Write("Error: Leave Description cant be more than 500.");
                return;
            }

            ess.ELR_EMP_ID = Request.Form["EMPCode"];

            essLeaveRequestHeader.ELR_FROM_DT = ess.ELR_FROM_DT;
            essLeaveRequestHeader.ELR_TO_DT = ess.ELR_TO_DT;
            essLeaveRequestHeader.ELR_LEAVE_ID = ess.ELR_LEAVE_ID;
            essLeaveRequestHeader.ELR_SUBJ = ess.ELR_SUBJ;
            essLeaveRequestHeader.ELR_BODY = ess.ELR_BODY;

            essLeaveRequestHeader.ELR_EMP_ID = ess.ELR_EMP_ID;


            essLeaveRequestHeader.ELR_SERVICE_TYPE = ESSServices.LeaveCarryForward;
            ess.ELR_SERVICE_TYPE = ESSServices.LeaveCarryForward;

            essLeaveRequestHeader.ELR_CREATED_USER_ID = Request.Form["UserId"];

            if (ess.ELR_FROM_DT == null || ess.ELR_TO_DT == null)
            {
                Response.Write("Error: Enter the from date and to date values");
                return;
            }
            //empActingHidden
            ess.ELR_NOTIFY_TO = Request.Form["empActingHidden"];

            dbaccess GetDBObject = new dbaccess();
           
            SaveDocs("15");
            
            if (essLeaveRequestDA.Apply("", false, "01"))
            {
                WriteSuccessMessage("The request is sent.");
            }
            else
            {
                WriteErrorMessage("Error: Unable to send the request.");
            }


        }

        void SaveLeavePlanner()
        {

            var successYN = false;

            var fromDate = Request.Form["ESSfromDate"].ToArabicDate();
            var toDate = Request.Form["ESStoDate"].ToArabicDate();
            var Compcode = user.Login_Company_Code;
            var Leavetype = Request.QueryString["leavetype"];
            var StartDay = "N";// ((Request.Form["chkFirst"] == "true") ? "Y" : "N");
            var EndDay = "N";// ((Request.Form["chkSecond"] == "true") ? "Y" : "N");

            
            if (Request.Form["chkIsFirstDayOff"] != null && Request.Form["chkIsFirstDayOff"] == "on")
            {
                StartDay = "Y";
            }
            if (Request.Form["chkIsSecondDayOff"] != null && Request.Form["chkIsSecondDayOff"] == "on")
                EndDay = "Y";
            var empCode = "";
            if (!string.IsNullOrEmpty(Request["empToApplyLeaveHidden"]))
            {
                empCode = Request["empToApplyLeaveHidden"];
            }
            else
                empCode = user.Emp_Code;

            LogError.WriteToLogFile("", false, "empCode" + empCode);

            List<AppCode.Pro_Parameters> parameters = new List<AppCode.Pro_Parameters>();
            parameters.Add(new AppCode.Pro_Parameters { _par_name = "P_COMP_CODE", _par_value = Compcode });
            parameters.Add(new AppCode.Pro_Parameters { _par_name = "P_EMP_ID", _par_value = empCode });
            parameters.Add(new AppCode.Pro_Parameters { _par_name = "P_LEAVE_TYPE", _par_value = Leavetype });
            parameters.Add(new AppCode.Pro_Parameters { _par_name = "P_LEAVE_START_DATE", _par_data_type = "Date", _par_value = fromDate });
            parameters.Add(new AppCode.Pro_Parameters { _par_name = "P_LEAVE_END_DATE", _par_data_type = "Date", _par_value = toDate });
            parameters.Add(new AppCode.Pro_Parameters { _par_name = "P_START_DAY_HALF", _par_value = StartDay });
            parameters.Add(new AppCode.Pro_Parameters { _par_name = "P_END_DAY_HALF", _par_value = EndDay });

            var tot_days = new AppCode.Pro_Parameters { _par_name = "P_TOT_DAYS", _par_value = 0D, _par_in_out = "OUT", _par_data_type = "NUMBER" };
            parameters.Add(tot_days);
            var is_elgible = new AppCode.Pro_Parameters { _par_name = "P_IS_ELIGIBLE", _par_value = "", _par_in_out = "OUT", _par_data_type = "VARCHAR2" };
            parameters.Add(is_elgible);
            var status_leave = new AppCode.Pro_Parameters { _par_name = "P_STATUS_TEXT", _par_value = "", _par_in_out = "OUT", _par_data_type = "VARCHAR2" };
            parameters.Add(status_leave);


            AppCode.dbaccess db = new dbaccess();
            db.executeProcedure("PAY_GET_LEAVE_ELIG_DETAILS", parameters);

            LogError.WriteToLogFile("", false, "empCode::" + empCode);


            if (is_elgible._par_value != null)
            {

                successYN = is_elgible._par_value.ToString().Equals("YES");
            }

            if (!successYN)
            {
                var strmsg = status_leave._par_value.ToString();
                WriteErrorMessage("Error:" + strmsg);

                return;
            }


            essLeaveRequestDA = new EssLeaveRequestDA();
            essLeaveRequestHeader.LeaveReqDetails.Add(new EssLeaveRequest
            {
                ELR_LEAVE_REQ_ID = (essLeaveRequestHeader.LeaveReqDetails.Count() > 0 ? essLeaveRequestHeader.LeaveReqDetails.Max(m => m.ELR_LEAVE_REQ_ID) + 1 : 1),
                ELR_SERVICE_TYPE = ESSServices.LeavePlanner
            });

            EssLeaveRequest ess = essLeaveRequestHeader.LeaveReqDetails[0];


            ess.ELR_LEAVE_ID = Request.Form["ddlLeaveType"];
            ess.ELR_FROM_DT = Request.Form["ESSfromDate"].ToArabicDate();
            ess.ELR_TO_DT = Request.Form["ESStoDate"].ToArabicDate();
            ess.ELR_TOT_DAYS = Request.Form["leaveDaysTotal1"].ToDouble();
            // save apply empcode only when empcode is not same as login code
            if (empCode != user.Emp_Code)
                ess.ELR_APPLY_EMP_CODE = user.Emp_Code;


            var trueFalse = false;
            if (Request.Form["chkIsFirstDayOff"] != null && Request.Form["chkIsFirstDayOff"] == "on")
            {
                trueFalse = true;
            }
            ess.ELR_START_DAY_HALF = trueFalse;

            if (ess.ELR_TOT_DAYS <= 0)
            {
                WriteErrorMessage("Error: Total days is 0. Kindly reload the page.");
                return;
            }

            trueFalse = false;
            if (Request.Form["chkIsSecondDayOff"] != null && Request.Form["chkIsSecondDayOff"] == "on")
            {
                trueFalse = true;
            }

            ess.ELR_END_DAY_HALF = trueFalse;


            if (Request.Form["leavedesc"].Trim() == "")
                ess.ELR_BODY = ess.ELR_SUBJ = "Leave";
            else
                ess.ELR_BODY = ess.ELR_SUBJ = Request.Form["leavedesc"];



            ess.ELR_EMP_ID = empCode;

            essLeaveRequestHeader.ELR_FROM_DT = ess.ELR_FROM_DT;
            essLeaveRequestHeader.ELR_TO_DT = ess.ELR_TO_DT;
            essLeaveRequestHeader.ELR_LEAVE_ID = ess.ELR_LEAVE_ID;
            essLeaveRequestHeader.ELR_SUBJ = ess.ELR_SUBJ;
            essLeaveRequestHeader.ELR_BODY = ess.ELR_BODY;

            essLeaveRequestHeader.ELR_EMP_ID = ess.ELR_EMP_ID;


            essLeaveRequestHeader.ELR_SERVICE_TYPE = ESSServices.LeavePlanner;
            ess.ELR_SERVICE_TYPE = ESSServices.LeavePlanner;

            essLeaveRequestHeader.ELR_CREATED_USER_ID = Request.Form["UserId"];

            if (ess.ELR_SUBJ.Length > 499)
            {
                WriteErrorMessage("Error: Total characters cant be more than 500.");
                return;
            }

            DateTime fromDate1 = ess.ELR_FROM_DT.ToString().ToArabicDate(); DateTime toDate1 = ess.ELR_TO_DT.ToString().ToArabicDate();

            var strexistlp = @" SELECT ELR_LEAVE_REQ_HEAD_ID FROM ESS_LEAVE_REQ WHERE ELR_SERVICE_TYPE=37 AND ELR_LEAVE_ID='" + ess.ELR_LEAVE_ID + @"' AND   ELR_REQ_STATUS_ID=3 
           AND ((ELR_FROM_DT BETWEEN to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy') OR
           ELR_TO_DT BETWEEN to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy')) OR
               (ELR_FROM_DT < to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy') < ELR_TO_DT))";
                          
           log_error.write_to_log_file("Leave Planner exist", "false", strexistlp);
            var ExistlpId = db.execute_scalar(strexistlp);
            if (ExistlpId.ToString()!="")
            {
                WriteErrorMessage("Error: already Exist the leave planner.");
                return;
            }

            var strexistleave = @" SELECT ELR_LEAVE_REQ_HEAD_ID FROM ESS_LEAVE_REQ WHERE ELR_SERVICE_TYPE=1 AND ELR_LEAVE_ID='" + ess.ELR_LEAVE_ID + @"' AND   ELR_REQ_STATUS_ID NOT IN(5,11)
           AND ((ELR_FROM_DT BETWEEN to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy') OR
           ELR_TO_DT BETWEEN to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy')) OR
               (ELR_FROM_DT < to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy') < ELR_TO_DT))";

            log_error.write_to_log_file("Leave exist", "false", strexistleave);
            var Existleaveid = db.execute_scalar(strexistleave);
            if (ExistlpId.ToString() != "")
            {
                WriteErrorMessage("Error: already Exist the leave request.");
                return;
            }
            DataTable dtleave = new DataTable();
            dtleave = db.execute_query_retun_datatable(@"
            SELECT   Nvl( PLTM_ACTING_MNG_NOTIFY,'Y')  manager, Nvl( PLTM_ATTH_MANDATORY,'Y') atth,Nvl( PLTM_IS_DESC_REQ,'Y') desp
   
            FROM PPM_LEAVE_TYPE_MASTER where PLTM_LEAVE_TYPE_CODE ='" + ess.ELR_LEAVE_ID + @"'");

            if (dtleave.Rows.Count > 0)
            {
                //  Nvl( PLTM_DESC_MAND_YN,'Y') desp

                var LeaveDesc = dtleave.Rows[0]["desp"].ToString();
                if (LeaveDesc == "Y")
                {
                    // check if desc               

                    if (ess.ELR_BODY.Trim() == "")
                    {
                        WriteErrorMessage("Error: Decription is mandatory for leave.");
                        return;
                    }


                }

                var isAttachmentMandotry = dtleave.Rows[0]["atth"].ToString();
                if (isAttachmentMandotry == "Y")
                {
                    // check if attachment is exists

                    HttpPostedFile file = HttpContext.Current.Request.Files["fileInput"];

                    if (file.ContentLength == 0)
                    {
                        WriteErrorMessage("Error: Attachment is mandatory for LeavePlanner.");
                        return;
                    }


                }
            }

            if (ess.ELR_FROM_DT == null || ess.ELR_TO_DT == null)
            {
                WriteErrorMessage("Error: Enter the from date and to date values");
                return;
            }
            //empActingHidden
            ess.ELR_NOTIFY_TO = Request.Form["empActingHidden"];
            var actingEnabled = db.execute_scalar(@"SELECT 	 PLTM_ACTING_MNG_NOTIFY FROM PPM_LEAVE_TYPE_MASTER WHERE PLTM_LEAVE_TYPE_CODE='" + ess.ELR_LEAVE_ID + @"'");
            if (!string.IsNullOrEmpty(actingEnabled) && actingEnabled == "Y" && string.IsNullOrEmpty(ess.ELR_NOTIFY_TO))
            {
                WriteErrorMessage("Error: Kindly select acting manager.");
                return;
            }

            if (!string.IsNullOrEmpty(actingEnabled) && actingEnabled == "N")
            {
                ess.ELR_NOTIFY_TO = "N";
            }

            if (ess.ELR_FROM_DT.Value > ess.ELR_TO_DT.Value)
            {
                WriteErrorMessage("Error: From date should be greater than to date.");
                return;
            }

            SaveDocs("37", empCode);

            var isLeaveAttchMandotry = db.execute_scalar(@" SELECT 
     nvl(PLTM_ATTH_MANDATORY,'N') 
FROM PPM_LEAVE_TYPE_MASTER  where PLTM_LEAVE_TYPE_CODE='" + ess.ELR_LEAVE_ID + @"' ").Equals("Y");


            if (isLeaveAttchMandotry)
            {
                var oldFolderPath = HttpContext.Current.Request.MapPath("~/ESS/Docs/") + "/" + ess.ELR_EMP_ID + "/"
               + "/" + (int)ess.ELR_SERVICE_TYPE + "/" + HttpContext.Current.Session.SessionID;

                AppCode.LogError.WriteToLogFile("oldFolderPath", false, oldFolderPath.ToString());
                var filesAttached = false;
                if (Directory.Exists(oldFolderPath))
                {
                    var files = Directory.GetFiles(oldFolderPath);

                    if (files != null && files.Count() > 0)
                    {
                        filesAttached = true;
                    }
                }

                if (filesAttached == false)
                {
                    WriteErrorMessage("Error: attachment is mandatory.");
                    return;
                }
            }


            if (essLeaveRequestDA.Apply("", false, "01"))
            {
                WriteSuccessMessage("The request is sent.");
                //ks.showAlert("alert('The request is sent.'); window.location='" + ResolveUrl(Request.RawUrl) + @"';", Page);
            }
            else
            {
                WriteErrorMessage("Error: Unable to send the request.");
            }
        }
        void SaveLeave()
        {
            var successYN = false;

            var fromDate = Request.Form["ESSfromDate"].ToArabicDate();
            var toDate = Request.Form["ESStoDate"].ToArabicDate();
            var Compcode = user.Login_Company_Code;
            var Leavetype = Request.QueryString["leavetype"];
            var StartDay = "N";// ((Request.Form["chkFirst"] == "true") ? "Y" : "N");
            var EndDay = "N";// ((Request.Form["chkSecond"] == "true") ? "Y" : "N");

            var Hospital = Request["txthospital"];
            var Doctor = Request["txtdocname"];

            if (Request.Form["chkIsFirstDayOff"] != null && Request.Form["chkIsFirstDayOff"] == "on")
            {
                StartDay = "Y";
            }
            if (Request.Form["chkIsSecondDayOff"] != null && Request.Form["chkIsSecondDayOff"] == "on")
                EndDay = "Y";
            var empCode = "";
            if (!string.IsNullOrEmpty(Request["empToApplyLeaveHidden"]))
            {
                empCode = Request["empToApplyLeaveHidden"];
            }
            else
                empCode = user.Emp_Code;

            LogError.WriteToLogFile("", false, "empCode" + empCode);

            List<AppCode.Pro_Parameters> parameters = new List<AppCode.Pro_Parameters>();
            parameters.Add(new AppCode.Pro_Parameters { _par_name = "P_COMP_CODE", _par_value = Compcode });
            parameters.Add(new AppCode.Pro_Parameters { _par_name = "P_EMP_ID", _par_value = empCode });
            parameters.Add(new AppCode.Pro_Parameters { _par_name = "P_LEAVE_TYPE", _par_value = Leavetype });
            parameters.Add(new AppCode.Pro_Parameters { _par_name = "P_LEAVE_START_DATE", _par_data_type = "Date", _par_value = fromDate });
            parameters.Add(new AppCode.Pro_Parameters { _par_name = "P_LEAVE_END_DATE", _par_data_type = "Date", _par_value = toDate });
            parameters.Add(new AppCode.Pro_Parameters { _par_name = "P_START_DAY_HALF", _par_value = StartDay });
            parameters.Add(new AppCode.Pro_Parameters { _par_name = "P_END_DAY_HALF", _par_value = EndDay });

            var tot_days = new AppCode.Pro_Parameters { _par_name = "P_TOT_DAYS", _par_value = 0D, _par_in_out = "OUT", _par_data_type = "NUMBER" };
            parameters.Add(tot_days);
            var is_elgible = new AppCode.Pro_Parameters { _par_name = "P_IS_ELIGIBLE", _par_value = "", _par_in_out = "OUT", _par_data_type = "VARCHAR2" };
            parameters.Add(is_elgible);
            var status_leave = new AppCode.Pro_Parameters { _par_name = "P_STATUS_TEXT", _par_value = "", _par_in_out = "OUT", _par_data_type = "VARCHAR2" };
            parameters.Add(status_leave);


            AppCode.dbaccess db = new dbaccess();
            db.executeProcedure("PAY_GET_LEAVE_ELIG_DETAILS", parameters);

            LogError.WriteToLogFile("", false, "empCode::" + empCode);




            if (is_elgible._par_value != null)
            {

                successYN = is_elgible._par_value.ToString().Equals("YES");
            }

            if (!successYN)
            {
                var strmsg = status_leave._par_value.ToString();
                WriteErrorMessage("Error:" + strmsg);

                return;
            }


            essLeaveRequestDA = new EssLeaveRequestDA();
            essLeaveRequestHeader.LeaveReqDetails.Add(new EssLeaveRequest
            {
                ELR_LEAVE_REQ_ID = (essLeaveRequestHeader.LeaveReqDetails.Count() > 0 ? essLeaveRequestHeader.LeaveReqDetails.Max(m => m.ELR_LEAVE_REQ_ID) + 1 : 1),
                ELR_SERVICE_TYPE = ESSServices.Leave
            });

            EssLeaveRequest ess = essLeaveRequestHeader.LeaveReqDetails[0];

            ess.ELR_VEH_PURPOSE = Hospital;
            ess.ELR_VEH_OPERATOR = Doctor;
            ess.ELR_LEAVE_ID = Request.Form["ddlLeaveType"];
            ess.ELR_FROM_DT = Request.Form["ESSfromDate"].ToArabicDate();
            ess.ELR_TO_DT = Request.Form["ESStoDate"].ToArabicDate();
            ess.ELR_TOT_DAYS = tot_days._par_value.ToString().ToDouble();// Request.Form["leaveDaysTotal1"].ToDouble();
            // save apply empcode only when empcode is not same as login code
            if (empCode != user.Emp_Code)
                ess.ELR_APPLY_EMP_CODE = user.Emp_Code;

            // 
            double fraction = ess.ELR_TOT_DAYS % 1; // Extract fractional part
            if (fraction != 0 && fraction != 0.5)
            {

                WriteErrorMessage("Error: Fraction issue, kindly contact admin or reload the page, date setting causing this error.");
                string dataToWrite = @"Fraction error, ess.ELR_FROM_DT " + ess.ELR_FROM_DT + "ess.ELR_TO_DT" + ess.ELR_TO_DT + "ess.ELR_TOT_DAYS" + ess.ELR_TOT_DAYS+ "empCode"+ empCode;
                DataAccess.LogError.WriteTOSpecificFile("LeaveValidationError.txt", "Fraction issue, kindly contact admin or reload the page, date setting causing this error."+ dataToWrite);

                return;
            }

                var trueFalse = false;
            if (Request.Form["chkIsFirstDayOff"] != null && Request.Form["chkIsFirstDayOff"] == "on")
            {
                trueFalse = true;
            }
            ess.ELR_START_DAY_HALF = trueFalse;

            if (ess.ELR_TOT_DAYS <= 0)
            {
                string dataToWrite = @"Fraction error, ess.ELR_FROM_DT " + ess.ELR_FROM_DT + "ess.ELR_TO_DT" + ess.ELR_TO_DT + "ess.ELR_TOT_DAYS" + ess.ELR_TOT_DAYS+ "empCode"+ empCode;
                DataAccess.LogError.WriteTOSpecificFile("ValidationFailedError.txt", "Fraction issue, kindly contact admin or reload the page, date setting causing this error."+ dataToWrite);
                WriteErrorMessage("Error: Total days is 0. Kindly reload the page.");
                return;
            }

            trueFalse = false;
            if (Request.Form["chkIsSecondDayOff"] != null && Request.Form["chkIsSecondDayOff"] == "on")
            {
                trueFalse = true;
            }

            ess.ELR_END_DAY_HALF = trueFalse;


            if (Request.Form["leavedesc"].Trim() == "")
                ess.ELR_BODY = ess.ELR_SUBJ = "Leave";
            else
                ess.ELR_BODY = ess.ELR_SUBJ = Request.Form["leavedesc"];



            ess.ELR_EMP_ID = empCode;

            essLeaveRequestHeader.ELR_FROM_DT = ess.ELR_FROM_DT;
            essLeaveRequestHeader.ELR_TO_DT = ess.ELR_TO_DT;
            essLeaveRequestHeader.ELR_LEAVE_ID = ess.ELR_LEAVE_ID;
            essLeaveRequestHeader.ELR_SUBJ = ess.ELR_SUBJ;
            essLeaveRequestHeader.ELR_BODY = ess.ELR_BODY;

            essLeaveRequestHeader.ELR_EMP_ID = ess.ELR_EMP_ID;
            essLeaveRequestHeader.ELR_VEH_PURPOSE = ess.ELR_VEH_PURPOSE;
            essLeaveRequestHeader.ELR_VEH_OPERATOR = ess.ELR_VEH_OPERATOR;

            essLeaveRequestHeader.ELR_SERVICE_TYPE = ESSServices.Leave;
            ess.ELR_SERVICE_TYPE = ESSServices.Leave;

            essLeaveRequestHeader.ELR_CREATED_USER_ID = Request.Form["UserId"];

            if (ess.ELR_SUBJ.Length > 499)
            {
                WriteErrorMessage("Error: Total characters cant be more than 500.");
                return;
            }

            if (string.IsNullOrEmpty(ess.ELR_LEAVE_ID))
            {
                WriteErrorMessage("Error: Leave type cant be empty or null");
                return;
            }

            if (string.IsNullOrEmpty(essLeaveRequestHeader.ELR_CREATED_USER_ID))
            {
                WriteErrorMessage("Error: User id is empty. Please reload the page");
                return;
            }


            DataTable dtleave = new DataTable();
            dtleave = db.execute_query_retun_datatable(@"
SELECT   Nvl( PLTM_ACTING_MNG_NOTIFY,'Y')  manager, Nvl( PLTM_ATTH_MANDATORY,'Y') atth,Nvl( PLTM_IS_DESC_REQ,'Y') desp
   
FROM PPM_LEAVE_TYPE_MASTER where PLTM_LEAVE_TYPE_CODE ='" + ess.ELR_LEAVE_ID + @"'
");
            if (dtleave.Rows.Count > 0)
            {
                //  Nvl( PLTM_DESC_MAND_YN,'Y') desp

                var LeaveDesc = dtleave.Rows[0]["desp"].ToString();
                if (LeaveDesc == "Y")
                {
                    // check if desc               

                    if (ess.ELR_BODY.Trim() == "")
                    {
                        WriteErrorMessage("Error: Decription is mandatory for leave.");
                        return;
                    }


                }

                var isAttachmentMandotry = dtleave.Rows[0]["atth"].ToString();
                if (isAttachmentMandotry == "Y")
                {
                    // check if attachment is exists

                    HttpPostedFile file = HttpContext.Current.Request.Files["fileInput"];

                    if (file.ContentLength == 0)
                    {
                        WriteErrorMessage("Error: Attachment is mandatory for leave.");
                        return;
                    }


                }
            }

            if (ess.ELR_FROM_DT == null || ess.ELR_TO_DT == null)
            {
                WriteErrorMessage("Error: Enter the from date and to date values");
                return;
            }
            //empActingHidden
            ess.ELR_NOTIFY_TO = Request.Form["empActingHidden"];
            var actingEnabled = db.execute_scalar(@"SELECT 	 PLTM_ACTING_MNG_NOTIFY FROM PPM_LEAVE_TYPE_MASTER WHERE PLTM_LEAVE_TYPE_CODE='" + ess.ELR_LEAVE_ID + @"'");
            if (!string.IsNullOrEmpty(actingEnabled) && actingEnabled == "Y" && string.IsNullOrEmpty(ess.ELR_NOTIFY_TO))
            {
                WriteErrorMessage("Error: Kindly select acting manager.");
                return;
            }

            if (!string.IsNullOrEmpty(actingEnabled) && actingEnabled == "N")
            {
                ess.ELR_NOTIFY_TO = "N";
            }
          
            if (ess.ELR_FROM_DT.Value > ess.ELR_TO_DT.Value)
            {
                WriteErrorMessage("Error: From date should be greater than to date.");
                return;
            }
            
            SaveDocs("1", empCode);

            var isLeaveAttchMandotry = db.execute_scalar(@" SELECT 
     nvl(PLTM_ATTH_MANDATORY,'N') 
FROM PPM_LEAVE_TYPE_MASTER  where PLTM_LEAVE_TYPE_CODE='" + ess.ELR_LEAVE_ID + @"' ").Equals("Y");

 
            if (isLeaveAttchMandotry)
            {
                var oldFolderPath = HttpContext.Current.Request.MapPath("~/ESS/Docs/") + "/" + ess.ELR_EMP_ID + "/"
               + "/" + (int)ess.ELR_SERVICE_TYPE + "/" + HttpContext.Current.Session.SessionID;

                AppCode.LogError.WriteToLogFile("oldFolderPath", false, oldFolderPath.ToString());
                var filesAttached = false;
                if (Directory.Exists(oldFolderPath))
                {
                    var files = Directory.GetFiles(oldFolderPath);

                    if (files != null && files.Count() > 0)
                    {
                        filesAttached = true;
                    }
                }

                if (filesAttached == false)
                {
                    WriteErrorMessage("Error: attachment is mandatory.");
                    return;
                }
            }


            if (essLeaveRequestDA.Apply("", false, "01"))
            {
                WriteSuccessMessage("The request is sent.");
                //ks.showAlert("alert('The request is sent.'); window.location='" + ResolveUrl(Request.RawUrl) + @"';", Page);
            }
            else
            {
                WriteErrorMessage("Error: Unable to send the request.");
            }



        }

        void SaveAnnualLeave()
        {
            essLeaveRequestDA = new EssLeaveRequestDA();
            essLeaveRequestHeader.LeaveReqDetails.Add(new EssLeaveRequest
            {
                ELR_LEAVE_REQ_ID = (essLeaveRequestHeader.LeaveReqDetails.Count() > 0 ? essLeaveRequestHeader.LeaveReqDetails.Max(m => m.ELR_LEAVE_REQ_ID) + 1 : 1),
                ELR_SERVICE_TYPE = ESSServices.AnnualLeaveResume
            }); 
            //ELR_VEH_RETURN_ON
            EssLeaveRequest ess = essLeaveRequestHeader.LeaveReqDetails[0];
            ess.ELR_VOUCHER_NO = Request.QueryString["leavetype"];
            ess.ELR_LEAVE_ID = "AL";
            ess.ELR_FROM_DT = Request.QueryString["startdate"].ToArabicDate();
            ess.ELR_TO_DT = Request.QueryString["enddate"].ToArabicDate();
            //ess.ELR_TO_DT = Request.Form["tentdate"].ToArabicDate();
            ess.ELR_TOT_DAYS = Request.QueryString["leavedays"].ToDouble();
            ess.ELR_BODY = ess.ELR_SUBJ = Request.Form["leavedesc"];
            ess.ELR_VEH_RETURN_ON = Request.Form["ESStoDate"].ToArabicDate();
            ess.ELR_LOAN_REFERENCE = Request.QueryString["leaverefno"];
            ess.ELR_EMP_ID = Request.Form["EMPCode"];

            essLeaveRequestHeader.ELR_FROM_DT = ess.ELR_FROM_DT;
            essLeaveRequestHeader.ELR_TO_DT = ess.ELR_TO_DT;
            essLeaveRequestHeader.ELR_LEAVE_ID = ess.ELR_LEAVE_ID;
            essLeaveRequestHeader.ELR_SUBJ = ess.ELR_SUBJ;
            essLeaveRequestHeader.ELR_BODY = ess.ELR_BODY;
            essLeaveRequestHeader.ELR_EMP_ID = ess.ELR_EMP_ID;
            essLeaveRequestHeader.ELR_TOT_DAYS = ess.ELR_TOT_DAYS;

            essLeaveRequestHeader.ELR_SERVICE_TYPE = ESSServices.AnnualLeaveResume;
            ess.ELR_SERVICE_TYPE = ESSServices.AnnualLeaveResume;

            essLeaveRequestHeader.ELR_CREATED_USER_ID = Request.Form["UserId"];

            if (ess.ELR_FROM_DT == null || ess.ELR_TO_DT == null)
            {
                WriteErrorMessage("Error: Enter the from date and to date values");
                return;
            }

            if (ess.ELR_SUBJ.Length > 499)
            {
                WriteErrorMessage("Error: Total characters cant be more than 500.");
                return;
            }
            string strquery = "";


            DataTable dtrej = new DataTable();

            dtrej = db.execute_query_retun_datatable(@"
                           select * from ESS_LEAVE_REQ,AUTH_WF_HEADER
              where  ELR_LEAVE_REQ_HEAD_ID=AWH_UNIQUE_ID1 and AWH_FINAL_STATUS='R' AND ELR_EMP_ID='" + ess.ELR_EMP_ID + @"'    
                AND   ELR_SERVICE_TYPE=26 AND TO_CHAR(ELR_REQUESTED_DT,'DD/MM/YYYY')=TO_CHAR(SYSDATE,'DD/MM/YYYY')");


            if (dtrej.Rows.Count > 0)
            {
                WriteErrorMessage("Error: Rejected   Leave Resume can't apply on same date.");
                return;
            }

            DataTable dtleave = new DataTable();
            var fromdate = Request.QueryString["tentdate"];
            var todate = Request.Form["ESStoDate"];
            if (ess.ELR_FROM_DT > ess.ELR_TO_DT)
            {
                strquery = @"( ELR_FROM_DT           
            BETWEEN  TO_DATE('" + todate + @"','DD/MM/YYYY')   AND TO_DATE('" + fromdate + @"','DD/MM/YYYY') 
            OR
            ELR_TO_DT           
            BETWEEN  TO_DATE('" + todate + @"','DD/MM/YYYY')   AND TO_DATE('" + fromdate + @"','DD/MM/YYYY'))";
            }
            else
            {
                strquery = @"( ELR_FROM_DT           
            BETWEEN  TO_DATE('" + fromdate + @"','DD/MM/YYYY')   AND TO_DATE('" + todate + @"','DD/MM/YYYY') 
            OR
            ELR_TO_DT           
            BETWEEN  TO_DATE('" + fromdate + @"','DD/MM/YYYY')   AND TO_DATE('" + todate + @"','DD/MM/YYYY'))";

            }


            dtleave = db.execute_query_retun_datatable(@"
                           select * from ESS_LEAVE_REQ,AUTH_WF_HEADER
              where  ELR_LEAVE_REQ_HEAD_ID=AWH_UNIQUE_ID1 and AWH_FINAL_STATUS!='R' AND ELR_EMP_ID='" + ess.ELR_EMP_ID + @"'
                AND   ELR_SERVICE_TYPE=26 AND ELR_REQ_STATUS_ID IN(3) ");//  AND( " + strquery + @")


            if (dtleave.Rows.Count > 0)
            {
                WriteErrorMessage("Error: Previous Request is pending.");
                return;
            }
             
            SaveDocs("26");
            //ELR_VEH_RETURN_ON

            if (essLeaveRequestDA.Apply("", false, "01"))
            {
                WriteSuccessMessage("The request is sent.");
                //ks.showAlert("alert('The request is sent.'); window.location='" + ResolveUrl(Request.RawUrl) + @"';", Page);
            }
            else
            {
                WriteErrorMessage("Error: Unable to send the request.");
            }



        }


        void SaveLeaveCancel()
        {
            var successYN = false;
            var Reqid = Request.QueryString["Leavereqid"];


            var reqDetail = db.execute_query_retun_datatable(@"SELECT 
ELR_LEAVE_ID, to_char(ELR_FROM_DT,'dd-MM-yyyy') ELR_FROM_DT, 
    to_char(ELR_TO_DT,'dd-MM-yyyy') ELR_TO_DT, 
   ELR_TOT_DAYS
FROM ESS_LEAVE_REQ where ELR_LEAVE_REQ_HEAD_ID='" + Reqid + @"' ");


            if (string.IsNullOrEmpty(Reqid))
            {
                WriteErrorMessage("Error: leave req id is null or empty.");
                return;
            }
            var fromDate = Request.QueryString["fromdate"];
            var toDate = Request.QueryString["todate"];
            var Compcode = user.Login_Company_Code;
            var Leavetype = Request.QueryString["leavetype"];
            var Reason = Request.Form["leavereason"];
            var StartDay = ((Request.Form["chkFirst"] == "true") ? "Y" : "N");
            var EndDay = ((Request.Form["chkSecond"] == "true") ? "Y" : "N");

            if (reqDetail.Rows.Count==0)
            {
                WriteErrorMessage("Error: leave req id is null or empty, kindly reload the page.");
                return;
            }
            fromDate = reqDetail.Rows[0]["ELR_FROM_DT"].ToString();
            toDate = reqDetail.Rows[0]["ELR_TO_DT"].ToString();
            Leavetype = reqDetail.Rows[0]["ELR_LEAVE_ID"].ToString();

            essLeaveRequestDA = new EssLeaveRequestDA();
            essLeaveRequestHeader.LeaveReqDetails.Add(new EssLeaveRequest
            {
                ELR_LEAVE_REQ_ID = (essLeaveRequestHeader.LeaveReqDetails.Count() > 0 ? essLeaveRequestHeader.LeaveReqDetails.Max(m => m.ELR_LEAVE_REQ_ID) + 1 : 1),
                ELR_SERVICE_TYPE = ESSServices.LeaveCancellation
            });

            EssLeaveRequest ess = essLeaveRequestHeader.LeaveReqDetails[0];
            ess.ELR_TRAIN_PURPOSE = Reason.Replace("'","''");
            ess.ELR_LOAN_REFERENCE = Reqid;
            ess.ELR_LEAVE_ID = Leavetype;// Request.Form["ddlLeaveType"];
            ess.ELR_FROM_DT = fromDate.ToArabicDate();
            ess.ELR_TO_DT = toDate.ToArabicDate();
            ess.ELR_TOT_DAYS = Request.Form["leaveDaysTotal1"].ToDouble();
            var trueFalse = false;
            if (Request.Form["chkIsFirstDayOff"] != null && Request.Form["chkIsFirstDayOff"] == "on")
            {
                trueFalse = true;
            }
            ess.ELR_START_DAY_HALF = trueFalse;

            if (ess.ELR_TOT_DAYS <= 0)
            {
                WriteErrorMessage("Error: Total days is 0. Kindly reload the page.");
                return;
            }

            trueFalse = false;
            if (Request.Form["chkIsSecondDayOff"] != null && Request.Form["chkIsSecondDayOff"] == "on")
            {
                trueFalse = true;
            }

            ess.ELR_END_DAY_HALF = trueFalse;
            ess.ELR_BODY = ess.ELR_SUBJ = Request.QueryString["leavedesc"];


            ess.ELR_EMP_ID = Request.Form["EMPCode"];
            essLeaveRequestHeader.ELR_TRAIN_PURPOSE = ess.ELR_TRAIN_PURPOSE;
            essLeaveRequestHeader.ELR_LOAN_REFERENCE = ess.ELR_LOAN_REFERENCE;
            essLeaveRequestHeader.ELR_FROM_DT = ess.ELR_FROM_DT;
            essLeaveRequestHeader.ELR_TO_DT = ess.ELR_TO_DT;
            essLeaveRequestHeader.ELR_LEAVE_ID = ess.ELR_LEAVE_ID;
            essLeaveRequestHeader.ELR_SUBJ = ess.ELR_SUBJ;
            essLeaveRequestHeader.ELR_BODY = ess.ELR_BODY;

            essLeaveRequestHeader.ELR_EMP_ID = ess.ELR_EMP_ID;


            essLeaveRequestHeader.ELR_SERVICE_TYPE = ESSServices.LeaveCancellation;
            ess.ELR_SERVICE_TYPE = ESSServices.LeaveCancellation;

            essLeaveRequestHeader.ELR_CREATED_USER_ID = Request.Form["UserId"];

            if (ess.ELR_TRAIN_PURPOSE.Trim() == "")
            {
                WriteErrorMessage("Error: Reason for Cancellation is mandatory.");
                return;
            }

            if (ess.ELR_TRAIN_PURPOSE.Length > 499)
            {
                WriteErrorMessage("Error: Total characters cant be more than 500.");
                return;
            }

            if (ess.ELR_FROM_DT == null || ess.ELR_TO_DT == null)
            {
                WriteErrorMessage("Error: Enter the from date and to date values");
                return;
            }
            //empActingHidden
            ess.ELR_NOTIFY_TO = Request.Form["empActingHidden"];
            var Prevreq = db.execute_scalar(@"SELECT ELR_LEAVE_REQ_HEAD_ID FROM ESS_LEAVE_REQ WHERE ELR_SERVICE_TYPE=28 AND ELR_EMP_ID='" + ess.ELR_EMP_ID + "' AND ELR_REQ_STATUS_ID=3");
            if (!string.IsNullOrEmpty(Prevreq))
            {
                WriteErrorMessage("Error: Previous Request is Pending.");
                return;
            }


            if (ess.ELR_FROM_DT.Value > ess.ELR_TO_DT.Value)
            {
                WriteErrorMessage("Error: From date should be greater than to date.");
                return;
            }

            SaveDocs("28");
             


            if (essLeaveRequestDA.Apply("", false, "01"))
            {
                WriteSuccessMessage("The request is sent.");
                //ks.showAlert("alert('The request is sent.'); window.location='" + ResolveUrl(Request.RawUrl) + @"';", Page);
            }
            else
            {
                WriteErrorMessage("Error: Unable to send the request.");
            }



        }
        void SavePermission()
        {
            essLeaveRequestDA = new EssLeaveRequestDA();
            essLeaveRequestHeader.LeaveReqDetails.Add(new EssLeaveRequest
            {
                ELR_LEAVE_REQ_ID = (essLeaveRequestHeader.LeaveReqDetails.Count() > 0 ? essLeaveRequestHeader.LeaveReqDetails.Max(m => m.ELR_LEAVE_REQ_ID) + 1 : 1),
                ELR_SERVICE_TYPE = ESSServices.Permission
            }); ;

         
            EssLeaveRequest ess = essLeaveRequestHeader.LeaveReqDetails[0];

            TimeSpan fromTimeSpan = TimeSpan.Parse(Request.QueryString["ftime"].ToString());
            TimeSpan toTimeSpan = TimeSpan.Parse(Request.QueryString["ttime"].ToString());

            // Step 2: Calculate the difference
            TimeSpan difference = toTimeSpan - fromTimeSpan;

            // Step 3: Get the total difference in minutes
            int totalMinutesApplied = (int)difference.TotalMinutes;

            ess.ELR_LEAVE_ID = Request.Form["ddlLeaveType"];
            ess.ELR_FROM_DT = Request.Form["ESSfromDate"].ToArabicDate();
            ess.ELR_TO_DT = Request.Form["ESStoDate"].ToArabicDate();
            ess.ELR_TRAIN_PURPOSE = Request.Form["ESStoDate"] + ' ' + Request.QueryString["ftime"];
            ess.ELR_TRAIN_GOAL = Request.Form["ESStoDate"] + ' ' + Request.QueryString["ttime"];

            ess.ELR_BODY = ess.ELR_SUBJ = Request.Form["leavedesc"];

            ess.ELR_EMP_ID = Request.Form["EMPCode"];

            essLeaveRequestHeader.ELR_FROM_DT = ess.ELR_FROM_DT;
            essLeaveRequestHeader.ELR_TO_DT = ess.ELR_TO_DT;
            essLeaveRequestHeader.ELR_LEAVE_ID = ess.ELR_LEAVE_ID;
            essLeaveRequestHeader.ELR_SUBJ = ess.ELR_SUBJ;
            essLeaveRequestHeader.ELR_BODY = ess.ELR_BODY;
            essLeaveRequestHeader.ELR_TRAIN_PURPOSE = ess.ELR_TRAIN_PURPOSE;
            essLeaveRequestHeader.ELR_TRAIN_GOAL = ess.ELR_TRAIN_GOAL;
            essLeaveRequestHeader.ELR_EMP_ID = ess.ELR_EMP_ID;


            essLeaveRequestHeader.ELR_SERVICE_TYPE = ESSServices.Permission;
            ess.ELR_SERVICE_TYPE = ESSServices.Permission;

            essLeaveRequestHeader.ELR_CREATED_USER_ID = Request.Form["UserId"];

            if (ess.ELR_FROM_DT == null || ess.ELR_TO_DT == null)
            {
                WriteErrorMessage("Error: Enter the from date and to date values");
                return;
            }

            DataTable dtpermission = new DataTable();


            string strvalcheck = @"
                           select TO_DATE(ELR_TRAIN_PURPOSE,'DD/MM/YYYY HH24:MI:SS') FROMDATE,TO_DATE(ELR_TRAIN_GOAL,'DD/MM/YYYY HH24:MI:SS')TODATE,ELR_TRAIN_PURPOSE,ELR_TRAIN_GOAL from ess_leave_req,AUTH_WF_HEADER
              where  ELR_LEAVE_REQ_HEAD_ID=AWH_UNIQUE_ID1 and AWH_FINAL_STATUS!='R' and ELR_EMP_ID='" + ess.ELR_EMP_ID + @"'   
                AND   ELR_SERVICE_TYPE=3 AND to_char(ELR_FROM_DT,'DD/MM/YYYY')='" + Request.Form["ESSfromDate"] + @"' AND    elr_req_status_id IN(3,7,8) 
            AND to_char(ELR_TO_DT,'DD/MM/YYYY')='" + Request.Form["ESStoDate"] + @"' AND( 
            TO_DATE(ELR_TRAIN_PURPOSE,'DD/MM/YYYY HH24:MI:SS')            
            BETWEEN TO_DATE('" + ess.ELR_TRAIN_PURPOSE + @"','DD/MM/YYYY HH24:MI:SS') AND TO_DATE('" + ess.ELR_TRAIN_GOAL + @"','DD/MM/YYYY HH24:MI:SS')
            OR
            TO_DATE(ELR_TRAIN_GOAL,'DD/MM/YYYY HH24:MI:SS') 
            BETWEEN TO_DATE('" + ess.ELR_TRAIN_PURPOSE + @"','DD/MM/YYYY HH24:MI:SS') AND TO_DATE('" + ess.ELR_TRAIN_GOAL + @"','DD/MM/YYYY HH24:MI:SS'))";
            
            log_error.write_to_log_file("Permission", "true", strvalcheck);

            dtpermission = db.execute_query_retun_datatable(strvalcheck);
            if (dtpermission.Rows.Count > 0)
            {
                WriteErrorMessage("Error: Permission is already applied on the selected date..");
                return;
            }

            var PPT_ALLOWPAST_DAY = db.execute_scalar("select PPT_ALLOWPAST_DAY from PPM_PERMISSION_TYPES r WHERE PPT_PERMISSION_CODE='" + ess.ELR_LEAVE_ID + @"'");
            if (PPT_ALLOWPAST_DAY != "Y")
            {
                var FromDate = Request["ESSfromDate"].ToArabicDate();
                if (ess.ELR_FROM_DT < DateTime.Now.Date)
                {
                    WriteErrorMessage("You cant apply permission for past days");
                    return;
                }
            }

            var PPT_ALLOWED_PER_INSTANCE_IN_MINS = objDB.execute_scalar(@"SELECT PPT_ALLOWED_PER_INSTANCE_IN_MINS FAILED_CNT FROM PPM_PERMISSION_TYPES WHERE PPT_PERMISSION_CODE ='" + ess.ELR_LEAVE_ID + @"'  ").ToInt();


            if (PPT_ALLOWED_PER_INSTANCE_IN_MINS > 0 && totalMinutesApplied>PPT_ALLOWED_PER_INSTANCE_IN_MINS)
            {
                WriteErrorMessage("Error: You cannot apply permissions for more than " + PPT_ALLOWED_PER_INSTANCE_IN_MINS + @" minutes at a time .");
                return;
            }
 
            var MonthStartDate = new DateTime(ess.ELR_FROM_DT.Value.Year, ess.ELR_FROM_DT.Value.Month, 1);
            var MonthEndDate = new DateTime(ess.ELR_TO_DT.Value.Year, ess.ELR_TO_DT.Value.Month, DateTime.DaysInMonth(ess.ELR_TO_DT.Value.Year, ess.ELR_TO_DT.Value.Month));
            // checking no of mins allowed in a month
            Dictionary<string, int> YearMonthTotMins = new Dictionary<string, int>();

            for (int i = 0; i < (ess.ELR_TO_DT.Value - ess.ELR_FROM_DT.Value).TotalDays+1; i++)
            {
                var yyyyMM = ess.ELR_FROM_DT.Value.AddDays(i).ToString("yyyyMM");
                int totVal = 0;
                if(YearMonthTotMins.TryGetValue(yyyyMM, out totVal))
                {
                    // update dictionary item with
                     YearMonthTotMins[yyyyMM] = totVal + totalMinutesApplied;
                }
                else
                {
                    // add new item
                    YearMonthTotMins.Add(yyyyMM, totalMinutesApplied);
                }
            }
            var TotalHoursAllowedMonth = objDB.execute_scalar(@"select trunc(case when PPT_MAX_ALLOWED_IN_MONTH_MINS>0 then PPT_MAX_ALLOWED_IN_MONTH_MINS/ 60 else 0 end) from PPM_PERMISSION_TYPES where PPT_PERMISSION_CODE='" + ess.ELR_LEAVE_ID + @"' ").ToInt();
            if (YearMonthTotMins.Count() > 0 && TotalHoursAllowedMonth>0)
            {

                //PPT_MAX_ALLOWED_IN_MONTH_MINS, PPT_ALLOWED_PER_INSTANCE_MINS, PPT_ALLOWED_PER_INSTANCE_IN_MI
                foreach (var item in YearMonthTotMins)
                {
                    //YYYYMM, SUM(TOT_MINS1) TOT_MINS1, SUM(TOT_MINS1)/60 hours , min(PPT_MAX_ALLOWED_IN_MONTH_MINS) PPT_MAX_ALLOWED_IN_MONTH_MINS 

                    //SUM(TOT_MINS1) TOT_MINS1 , max(PPT_MAX_ALLOWED_IN_MONTH_MINS)
                    // If already applied permission hours greater than 
                    var QueryToCheckForMonth = @"
SELECT case when SUM(TOT_MINS1) + "+item.Value+@" >  max(PPT_MAX_ALLOWED_IN_MONTH_MINS) then 'FAILED' ELSE 'PASSED' END SUCCESS_STATUS FROM ( 
WITH CTE AS ( 
SELECT ELR_EMP_ID,
    TO_DATE( ELR_TRAIN_PURPOSE, 'DD-MM-YYYY HH24:MI') FROMDATE ,
    TO_DATE( ELR_TRAIN_GOAL, 'DD-MM-YYYY HH24:MI')  TODATE ,ELR_LEAVE_ID,
    PPT_PERMISSION_DESC, ELR_LEAVE_REQ_HEAD_ID, ELR_FROM_DT, ELR_TO_DT,ELR_IN1_A, ELR_OUT1_A, PPT_MAX_ALLOWED_IN_MONTH_MINS
FROM 
    ESS_LEAVE_REQ , PPM_PERMISSION_TYPES WHERE ELR_SERVICE_TYPE=3
    AND PPT_PERMISSION_CODE = ELR_LEAVE_ID  
    and elr_emp_id ='" + ess.ELR_EMP_ID + @"'
    and ELR_LEAVE_ID = '"+ess.ELR_LEAVE_ID+ @"' and elr_req_status_id IN (3, 7, 8)
) , CTE2 AS ( 
SELECT TO_CHAR(D1+LEVEL,'yyyymm') YYYYMM,D1+LEVEL DATE1,  D1, D2 , (D2-D1)+1 DAYS FROM (  
SELECT to_date('" +
MonthStartDate.ToString("dd-MM-yyyy") + @"','dd-mm-yyyy') D1, to_date('" +
MonthEndDate.ToString("dd-MM-yyyy") + @"','dd-mm-yyyy') D2 FROM Dual ) CONNECT BY LEVEL < (D2-D1)+1  )
    SELECT YYYYMM, 
    CASE WHEN DATE1 BETWEEN ELR_FROM_DT AND ELR_TO_DT THEN ( TODATE - FROMDATE) * 24 * 60 ELSE 0 END TOT_MINS1,
    ELR_EMP_ID,
    ELR_LEAVE_REQ_HEAD_ID, ELR_FROM_DT, ELR_TO_DT,   FROMDATE,  TODATE,    ELR_IN1_A, ELR_OUT1_A, 
    ( TODATE - FROMDATE) * 24 * 60 TOT_MINS , PPT_MAX_ALLOWED_IN_MONTH_MINS FROM CTE, CTE2
)  GROUP BY YYYYMM ";
                    LogError.WriteToLogFile("checking ", false, QueryToCheckForMonth);
                    var successStatus = objDB.execute_scalar(QueryToCheckForMonth  );
                    if(successStatus == "FAILED")
                    {

                        WriteErrorMessage("Error: Total number of hours allowed for a month is "+ TotalHoursAllowedMonth + @".");
                        return;
                    }

                }
            }
            if (ess.ELR_FROM_DT.Value > ess.ELR_TO_DT.Value)
            {
                WriteErrorMessage("Error: From date should be less than to date.");
                return;
            }

            SaveDocs("3");
             

            if (essLeaveRequestDA.Apply("", false, "01"))
            {
                WriteSuccessMessage("The request is sent.");
             }
            else
            {
                WriteErrorMessage("Error: Unable to send the request.");
            }


            //Response.Write("Saved successfully.");
        }

        void SaveCheckinout()
        {
            essLeaveRequestDA = new EssLeaveRequestDA();
            essLeaveRequestHeader.LeaveReqDetails.Add(new EssLeaveRequest
            {
                ELR_LEAVE_REQ_ID = (essLeaveRequestHeader.LeaveReqDetails.Count() > 0 ? essLeaveRequestHeader.LeaveReqDetails.Max(m => m.ELR_LEAVE_REQ_ID) + 1 : 1),
                ELR_SERVICE_TYPE = ESSServices.Checkinout
            }); ;

            EssLeaveRequest ess = essLeaveRequestHeader.LeaveReqDetails[0];
            var cdatetime = Request.Form["ESSfromDate"].ToString() + ' ' + Request.QueryString["ftime"].ToString();
            var checkstatus = Request.QueryString["status"].ToString();

            ess.ELR_TRAIN_PURPOSE = cdatetime;
            ess.ELR_FROM_DT = cdatetime.ToArabicDateNull();

            if (!ess.ELR_FROM_DT.HasValue)
            {
                WriteErrorMessage("Error: Time cant be empty");
                return;
            }
            ess.ELR_TO_DT = cdatetime.ToArabicDateNull();
            if (checkstatus == "Y")
            {
                ess.ELR_END_DAY_HALF = true; ess.ELR_LEAVE_ID = "IN";
            }
            else
            {
                ess.ELR_END_DAY_HALF = false; ess.ELR_LEAVE_ID = "OUT";
            }
            ess.ELR_VEH_OPERATOR = Request.QueryString["latitude"];
            ess.ELR_VEH_STAFF = Request.QueryString["longitude"];

            ess.ELR_BODY = ess.ELR_SUBJ = Request.Form["leavedesc"];

            ess.ELR_EMP_ID = Request.Form["EMPCode"];
            LogError.WriteToLogFile("essss", false, Newtonsoft.Json.JsonConvert.SerializeObject(ess));
            essLeaveRequestHeader.ELR_FROM_DT = ess.ELR_FROM_DT;
            essLeaveRequestHeader.ELR_TO_DT = ess.ELR_TO_DT;
            essLeaveRequestHeader.ELR_LEAVE_ID = ess.ELR_LEAVE_ID;
            essLeaveRequestHeader.ELR_SUBJ = ess.ELR_SUBJ;
            essLeaveRequestHeader.ELR_BODY = ess.ELR_BODY;
            essLeaveRequestHeader.ELR_END_DAY_HALF = ess.ELR_END_DAY_HALF;
            essLeaveRequestHeader.ELR_TRAIN_PURPOSE = ess.ELR_TRAIN_PURPOSE;
            essLeaveRequestHeader.ELR_TRAIN_GOAL = ess.ELR_TRAIN_PURPOSE;
            essLeaveRequestHeader.ELR_EMP_ID = ess.ELR_EMP_ID;

            essLeaveRequestHeader.ELR_VEH_OPERATOR = ess.ELR_VEH_OPERATOR;
            essLeaveRequestHeader.ELR_VEH_STAFF = ess.ELR_VEH_STAFF;

            essLeaveRequestHeader.ELR_SERVICE_TYPE = ESSServices.Checkinout;
            ess.ELR_SERVICE_TYPE = ESSServices.Checkinout;

            essLeaveRequestHeader.ELR_CREATED_USER_ID = Request.Form["UserId"];

            LogError.WriteToLogFile("essss", false, Newtonsoft.Json.JsonConvert.SerializeObject(ess));
            var esscheckin = db.execute_scalar(@"SELECT TAP_PARAVALUE FROM TAS_ATT_PARAM WHERE TAP_PARANAME = 'ESS_CHECKIN_YN'").ToString();
            if (esscheckin == "N")
            {
                WriteErrorMessage("Error: You aren't Eligible Check In/Out");
                return;
            }

            DataTable dtpermission = new DataTable();


            dtpermission = db.execute_query_retun_datatable(@"
                           select TO_DATE(ELR_TRAIN_PURPOSE,'DD/MM/YYYY HH24:MI:SS') FROMDATE,TO_DATE(ELR_TRAIN_GOAL,'DD/MM/YYYY HH24:MI:SS') TODATE,ELR_TRAIN_PURPOSE,ELR_TRAIN_GOAL from ess_leave_req,AUTH_WF_HEADER
              where  ELR_LEAVE_REQ_HEAD_ID=AWH_UNIQUE_ID1 and AWH_FINAL_STATUS!='R'   
                AND   ELR_SERVICE_TYPE=32 AND to_char(ELR_FROM_DT,'DD-MM-YYYY')='" + Request.Form["ESSfromDate"].ToString() + @"' AND   to_char(ELR_FROM_DT,'HH24:MI')='" + Request.QueryString["ftime"].ToString() + @"'");
             

            if (dtpermission.Rows.Count > 0)
            {
                //Response.Write("Error: Already exist this Request on this date & time.");
                //return;
            }





            if (ess.ELR_FROM_DT.Value > ess.ELR_TO_DT.Value)
            {
                WriteErrorMessage("Error: From date should be greater than to date.");
                return;
            }

            SaveDocs("32");



            if (essLeaveRequestDA.Apply("", false, "01"))
            {
                WriteSuccessMessage("The request is sent.");

            }
            else
            {
                WriteErrorMessage("Error: Unable to send the request.");
            }



        }
        void SaveCompoff()
        {
            string param_show_hide = "";

            essLeaveRequestDA = new EssLeaveRequestDA();
            essLeaveRequestHeader.LeaveReqDetails.Add(new EssLeaveRequest
            {
                ELR_LEAVE_REQ_ID = (essLeaveRequestHeader.LeaveReqDetails.Count() > 0 ? essLeaveRequestHeader.LeaveReqDetails.Max(m => m.ELR_LEAVE_REQ_ID) + 1 : 1),
                ELR_SERVICE_TYPE = ESSServices.CompoffRequest
            });

            EssLeaveRequest ess = essLeaveRequestHeader.LeaveReqDetails[0];


            ess.ELR_LEAVE_ID = Request.Form["ddlLeaveType"];
            ess.ELR_FROM_DT = Request.Form["ESSfromDate"].ToArabicDate();
            ess.ELR_TO_DT = Request.Form["ESStoDate"].ToArabicDate();
            ess.ELR_TOT_DAYS = Request.Form["leaveDaysTotal1"].ToDouble();
            var trueFalse = false;
            if (Request.Form["chkIsFirstDayOff"] != null && Request.Form["chkIsFirstDayOff"] == "on")
            {
                trueFalse = true;
            }
            ess.ELR_START_DAY_HALF = trueFalse;

            if (ess.ELR_TOT_DAYS <= 0)
            {
                Response.Write("Error: Total days is 0. Kindly reload the page.");
                return;
            }

            trueFalse = false;
            if (Request.Form["chkIsSecondDayOff"] != null && Request.Form["chkIsSecondDayOff"] == "on")
            {
                trueFalse = true;
            }

            ess.ELR_END_DAY_HALF = trueFalse;
            ess.ELR_BODY = ess.ELR_SUBJ = Request.Form["leavedesc"];


            ess.ELR_EMP_ID = Request.Form["EMPCode"];

            essLeaveRequestHeader.ELR_FROM_DT = ess.ELR_FROM_DT;
            essLeaveRequestHeader.ELR_TO_DT = ess.ELR_TO_DT;
            essLeaveRequestHeader.ELR_LEAVE_ID = ess.ELR_LEAVE_ID;
            essLeaveRequestHeader.ELR_SUBJ = ess.ELR_SUBJ; //= "Compoff Request";
            essLeaveRequestHeader.ELR_BODY = ess.ELR_BODY; //= "Compoff Request";

            essLeaveRequestHeader.ELR_EMP_ID = ess.ELR_EMP_ID;


            essLeaveRequestHeader.ELR_SERVICE_TYPE = ESSServices.CompoffRequest;
            ess.ELR_SERVICE_TYPE = ESSServices.CompoffRequest;

            essLeaveRequestHeader.ELR_CREATED_USER_ID = Request.Form["UserId"];

            param_show_hide = db.execute_scalar(@"SELECT TAP_PARAVALUE FROM TAS_ATT_PARAM WHERE TAP_PARANAME='REG_COFF_ATT_MANDYN'");

            var isAttachmentMandotry = db.execute_scalar(@"
SELECT 
    Nvl( PLTM_ATTH_MANDATORY,'N') 
FROM PPM_LEAVE_TYPE_MASTER where PLTM_LEAVE_TYPE_CODE ='" + ess.ELR_LEAVE_ID + @"'
    ");
            if (isAttachmentMandotry == "Y" || param_show_hide == "Y")
            {
                // check if attachment is exists
                HttpPostedFile file = HttpContext.Current.Request.Files["fileInput"];
                if (file.ContentLength == 0)
                {
                    WriteErrorMessage("Error: Attachment is mandatory for Compoff.");
                    return;
                }
            }
            if (ess.ELR_FROM_DT == null || ess.ELR_TO_DT == null)
            {
                WriteErrorMessage("Error: Enter the from date and to date values");
                return;
            }
            //empActingHidden
            ess.ELR_NOTIFY_TO = Request.Form["empActingHidden"];

        
            if (!ESSHelperFunctions.IsApplyInAdvacne(ess.ELR_LEAVE_ID))
            {
                if (ess.ELR_FROM_DT > DateTime.Now && ess.ELR_TO_DT > DateTime.Now)
                {
                    WriteErrorMessage("Error: Leave cannot be applied for future dates.");
                    return;
                }
            }
            if (ess.ELR_FROM_DT.Value > DateTime.Now || ess.ELR_TO_DT.Value > DateTime.Now)
            {
                WriteErrorMessage("Error: Date cant be future date.");
                return;
            }
            if (ess.ELR_SUBJ.Length > 499)
            {
                WriteErrorMessage("Error: Total characters cant be more than 500.");
                return;
            }
            if (ess.ELR_FROM_DT.Value > ess.ELR_TO_DT.Value)
            {
                WriteErrorMessage("Error: From date should be greater than to date.");
                return;
            }
            // validate number of days allowed for 
            var dateDiff = (DateTime.Now.Date - ess.ELR_TO_DT.Value.Date).TotalDays + 1;

            var claimWithIn = db.execute_scalar(@"SELECT  TAP_PARAVALUE FROM  TAS_ATT_PARAM WHERE TAP_PARANAME = 'COFF_CLAIM_WITH_IN'").ToDouble(); // 

            if (claimWithIn != 0 && dateDiff > claimWithIn)
            {
                WriteErrorMessage("Error: Compoff can not be applied after " + claimWithIn + " days ");
                return;
            }

            // CHECK for max comp  off
            var maxCompOffDays = db.execute_scalar(@"SELECT  TAP_PARAVALUE FROM  TAS_ATT_PARAM WHERE TAP_PARANAME = 'COMP_OFF_MAX_BAL'").ToDouble(); //COMP_OFF_MAX_BAL
            var totLeaveNotaproved = db.execute_scalar(@"
SELECT 
    nvl( sum(ELR_TOT_DAYS) ,0) 
FROM ESS_LEAVE_REQ
where
    ELR_EMP_ID='" + ess.ELR_EMP_ID + @"' and  ELR_LEAVE_ID= '" + ess.ELR_LEAVE_ID + @"' 
    and ELR_REQ_STATUS_ID    in ( 3,4) 
    and ELR_SERVICE_TYPE = 19
").ToDouble();

            var compOffBalance = db.execute_scalar(@"
SELECT 
    nvl ( PELD_LEAVE_AVAILED_DAYS,0) 
FROM PPM_EMPLOYEE_LEAVE_DETAILS  where PELD_CODE='" + ess.ELR_EMP_ID + @"'         and  PELD_LEAVE_TYPE = '" + ess.ELR_LEAVE_ID + @"' 
").ToDouble();


            if (maxCompOffDays != 0 && (ess.ELR_TOT_DAYS + compOffBalance + totLeaveNotaproved) > maxCompOffDays)
            {
                WriteErrorMessage("Error: Your compoff balance and requested days is more than " + maxCompOffDays);
                return;
            }

            //if (ESSHelperFunctions.IsLeaveDaysExceeded(ess.ELR_EMP_ID, ess.ELR_LEAVE_ID, (float)ess.ELR_TOT_DAYS, user.Login_Company_Code))
            //{
            //    Response.Write("Error: You have taken all the days for this leave type.");
            //    return;
            //}
            var QUERYAlreadyapplied = @"SELECT COUNT(*) FROM ESS_LEAVE_REQ ,
        (SELECT TO_DATE( '" + ess.ELR_FROM_DT.Value.ToString("dd/MM/yyyy") + @"', 'dd/MM/yyyy')  A ,  TO_DATE('" +
        ess.ELR_TO_DT.Value.ToString("dd/MM/yyyy") + @"' , 'dd/MM/yyyy')  B FROM DUAL) X
WHERE ELR_SERVICE_TYPE = 19
  AND  ELR_EMP_ID='" + ess.ELR_EMP_ID + @"' 
    AND ((ELR_FROM_DT BETWEEN X.A AND X.B OR
        ELR_TO_DT BETWEEN X.A AND X.B) OR 
          ( ELR_FROM_DT < X.A AND X.B < ELR_TO_DT ))
        AND ELR_REQ_STATUS_ID NOT IN ('1','5','6', '11')";

            if (objDB.record_found(QUERYAlreadyapplied) > 0)
            {
                WriteErrorMessage("Error: Leave is already applied on the selected days.");
                return;
            }
            
            SaveDocs("1");
            var isLeaveAttchMandotry = db.execute_scalar(@" SELECT 
     nvl(PLTM_ATTH_MANDATORY,'N') 
FROM PPM_LEAVE_TYPE_MASTER  where PLTM_LEAVE_TYPE_CODE='" + ess.ELR_LEAVE_ID + @"' ").Equals("Y");

            //AppCode.LogError.WriteToLogFile("isLeaveAttchMandotry", false, isLeaveAttchMandotry.ToString());

            if (isLeaveAttchMandotry)
            {
                var oldFolderPath = HttpContext.Current.Request.MapPath("~/ESS/Docs/") + "/" + ess.ELR_EMP_ID + "/"
               + "/" + (int)ess.ELR_SERVICE_TYPE + "/" + HttpContext.Current.Session.SessionID;
                var filesAttached = false;
                if (Directory.Exists(oldFolderPath))
                {
                    var files = Directory.GetFiles(oldFolderPath);

                    if (files != null && files.Count() > 0)
                    {
                        filesAttached = true;
                    }
                }

                if (filesAttached == false)
                {
                    WriteErrorMessage("Error: attachment is mandatory.");
                    return;
                }
            }


            if (essLeaveRequestDA.Apply("", false, "01"))
            {
                WriteSuccessMessage("The request is sent.");
                //ks.showAlert("alert('The request is sent.'); window.location='" + ResolveUrl(Request.RawUrl) + @"';", Page);
            }
            else
            {
                WriteErrorMessage("Error: Unable to send the request.");
            }

        }
        void LoadUser()
        {
            if (Session["userDetail"] != null)
            {
                user = (Session["userDetail"] as UserDatail);
            }
            else
            {

                if (Request.QueryString["Method"] == "AER_LOGIN" || Request.QueryString["Method"] == "USER_ROLES"
                    || Request.QueryString["Method"] == "APPOINTMENT_LINK"
                || Request.QueryString["Method"] == "AER_LOGIN2" 
                || Request.QueryString["Method"] == "Forgotpassword" 
                || Request.QueryString["Method"] == "USER_ROLES_ONTC"
                    )
                {


                    user = new UserDatail();
                    user.Emp_Code = "5070";
                    user.IsAdministrator = true;
                    currentempCode = user.Emp_Code;
                    user.Login_Company_Code = "01";
                }
                else
                {
                    ResponseWrite("Error: Session expired. Please login again.");
                    Response.End();
                }
            }

        }
        //[System.Web.Services.WebMethod]
        public string GetMyApproval()
        {

            LoadUser();
            string strsort = "", strexpr = "";
            string JSONresult = "my data...";
            var ReqType = Request.QueryString["ReqType"];
            var selBranch = Request.QueryString["selBranch"];
            var selLeaveType = Request.QueryString["selLeaveType"];
            // var from = Request.Form["EssfromDate"];
            // var to = Request.Form["EsstoDate"];
            if (ReqType == "ALL")
                ReqType = "";
            if (selLeaveType == "ALL")
                selLeaveType = "";
            ESSGetWFApprovalObject getWF = new ESSGetWFApprovalObject();
            var wfInstance = getWF.GetInstance();

            var empCode1 = Request["empact"].Trim();

            empCode1 = ((string.IsNullOrEmpty(empCode1) ? "ALL" : empCode1));
            selBranch = (string.IsNullOrEmpty(selBranch) ? "ALL" : selBranch);
             var dt = wfInstance.GetMyApproval(ReqType, selLeaveType);
            strsort = " ESS_REQ_TYPE1";

            if (dt != null && dt.Rows.Count > 0)
            {
                if (!dt.Columns.OfType<DataColumn>().ToList().Select(s => s.ColumnName).Contains("AWAL_OWNER_OF_WF"))
                {
                    dt.Columns.Add("AWAL_OWNER_OF_WF");
                }
                DataRow[] drs = null;
                if (selBranch != "ALL" && empCode1 != "ALL")
                {
                    drs = dt.Select(@" PEMP_EMP_CODE='" + empCode1 + "'  and  PEMP_EMP_BRANCH_CODE='" + selBranch + "'  ", strsort);//.CopyToDataTable();
                }
                else if (selBranch == "ALL" && empCode1 != "ALL")
                {
                    drs = dt.Select(@" PEMP_EMP_CODE='" + empCode1 + "'    ", strsort);//.CopyToDataTable();
                }
                else if (selBranch != "ALL" && empCode1 == "ALL")
                {
                    drs = dt.Select(@" PEMP_EMP_BRANCH_CODE='" + selBranch + "'    ", strsort);//.CopyToDataTable();
                }
                

                if (drs != null && drs.Count() > 0)
                {
                    dt = drs.CopyToDataTable();
                }
                else
                {
                    if (!(selBranch == "ALL" && empCode1 == "ALL"))
                        // no data found, so clear
                        dt.Rows.Clear();
                }
            }
         

            try
            {
                if (dt != null && dt.Rows.Count > 0)
                {
                   // dt.DefaultView.Sort = "ESS_REQ_TYPE1";
                    dt = dt.DefaultView.ToTable();
                }
            }
            catch (Exception e1)
            {

            }
            //}
 
            JSONresult = JsonConvert.SerializeObject(dt);
            Response.Write(JSONresult);
            return JSONresult;
        }

        public void SendForgotMail()
        {
            
            
            string Empusername = Request["UserName"];
            string Empemail= Request["UserEmail"];

            try
            {
                var emailmsg = Email.SendForgotpassword(Empusername, Empemail);
                if (emailmsg.IsSent)
                    ResponseWrite("your password has been sent Successfully");
                else
                    WriteErrorMessage(emailmsg.ErrorMessage);

            }
            catch (Exception e1)
            {
                WriteErrorMessage("Error" + e1.ToString());

            }


        }


        public string LoadEmployeeOfMonthDetails()
        {


            dbaccess ObjDb = new dbaccess();


            string strquery = @"SELECT
    'PHOTO' as FROM_TABLE,
EEOM_ROW_ID,
EOM_CAT_CODE,
   EOM_CAT_NAME,
to_char( to_date(EEOM_YEAR_MONTH,'yyyymm') ,'Mon yyyy','nls_date_language=english')  YOM  , 
VM.EMP_NAME ENAME,VM.PBM_BRANCH_NAME BRANCH_NAME,VM.PDPM_DEPARTMENT_DESC DEPT_NAME,VM.PDSM_DESIGNATION_DESC DESIGNATION_DESC,
    PEMP_EMP_PHOTO             
            from V_EMP_OF_MONTH VM , EMP_MASTER_DETAILS EM 
            WHERE  VM.PEMP_EMP_CODE=EM.PEMP_EMP_CODE
           ORDER BY  EEOM_YEAR_MONTH DESC";

            var dt = ObjDb.execute_query_retun_datatable(strquery);
            var JSONresult = "";
            if (dt.Rows.Count > 0)
            {
                foreach (DataRow item in dt.Rows)
                {
                    item["PEMP_EMP_PHOTO"] = ResolveUrl(item["PEMP_EMP_PHOTO"].ToString());


                    if (Directory.Exists(MapPath("~/EOM/")) && Directory.Exists(MapPath("~/EOM/" + item["EEOM_ROW_ID"].ToString())))
                    {
                        var files = Directory.GetFiles(MapPath("~/EOM/" + item["EEOM_ROW_ID"].ToString()));
                        if (files != null && files.Count() > 0)
                        {
                            var file2 = files[0];
                            item["FROM_TABLE"] = "MASTER";
                            item["PEMP_EMP_PHOTO"] = ResolveUrl("~/EOM/" + item["EEOM_ROW_ID"].ToString() + "/" +
                                (new FileInfo(file2)).Name);
                        }
                    }

                }
                JSONresult = JsonConvert.SerializeObject(dt);
                Response.Write(JSONresult);
                return JSONresult;
            }
            return JSONresult;
        }



        public string LoadmyRequest()
        {

            LoadUser();

            string JSONresult = "my data...";
            var ReqType = Request.QueryString["ReqType"];
            if (ReqType == "ALL")
                ReqType = "";
            var serviceCategory = Request.QueryString["serviceCategory"];
            if (string.IsNullOrEmpty(serviceCategory))
            {
                serviceCategory = "";
            }
            var dt = MyReqData(user.Emp_Code, user.IsAdministrator, ReqType, serviceCategory);


            JSONresult = JsonConvert.SerializeObject(dt);
            Response.Write(JSONresult);
            return JSONresult;
        }
        DataTable QueryNotifications(string empCode, bool isAdmin, string serviceTypeId)
        {
            string where1 = "";

            if (!isAdmin)
            {
                where1 = @"      and exists( SELECT 1 FROM AUTH_WF_APPROVER_LOG where AWAL_WF_DET_ROW_ID = AWDD_ROW_ID and AWAL_COMP_CODE='01' and AWAL_EMP_CODE='" + empCode + @"') ";
            }
            if (!string.IsNullOrEmpty(serviceTypeId))
            {
                where1 += " and EST_SERVICE_ID='" + serviceTypeId + @"' ";

            }
            return db.execute_query_retun_datatable(@"
SELECT 
     ESS_LEAVE_REQ_DETAILS_ALL.*,
     TO_CHAR(ESS_LEAVE_REQ_DETAILS_ALL.ELRH_REQUESTED_DT,'DD-Mon-YYYY','nls_date_language=english') DT1,
   
CASE WHEN STATUS =3  THEN 
    'Waiting For ' || Replace((AEG_DESCRIPTION),'Waiting For','') ELSE  CURR_STATUS_NAME END CURR_STATUS_NAME_1,
    case when est_service_id = 1 then 
        PLTM_LEAVE_TYPE_DESC
      when est_service_id = 5 then     
        ATT_REASON_DESC1 
      when est_service_id = 7 then
            ES_DESC
when est_service_id = 13 then
      AL_LETTER_NAME
            ELSE
            EST_SERVICE_DESC END AS ESS_REQ_TYPE1
    
   
FROM 
    ESS_LEAVE_REQ_DETAILS_ALL,AUTH_WF_HEADER, AUTH_WF_DOC_DETAIL,AUTH_EMP_GROUP
    where
        AWDD_ROW_ID = AWH_CURRENT_CONFIG_ROW_ID
       AND to_char(LEAVE_REQ_ID) = to_char(AWH_UNIQUE_ID1) AND to_char(LEAVE_REQ_ID) = to_char(AWH_UNIQUE_ID2)
        and AEG_EMP_GROUP_ID=AWDD_GROUP_ID
      --and ELR_COMP_CODE1 ='01' 
    and ELRH_REQ_STATUS_ID in (3,4)
    and ELRH_SERVICE_TYPE not in (41)
    --AND EST_SERVICE_ID =''
    " + where1 + " order by LEAVE_REQ_ID desc ");
        }

        DataTable MyReqData(string empCode, bool isAdmin, string serviceTypeId, string serviceCategory)
        {
            string where1 = "";
 
            Dictionary<string, object> keyValueParameters = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(serviceTypeId))
            {
                where1 += " and EST_SERVICE_ID=:serviceTypeId ";
                keyValueParameters.Add("serviceTypeId", serviceTypeId);

            }
            else
            {
                // others selected, so dont show services
                if (serviceCategory != "")
                {
                    where1 += " and ES_CATEGORY_ID=:serviceCategory ";
                    keyValueParameters.Add("serviceCategory", serviceCategory);
                }
                else
                {
                    where1 += " and EST_SERVICE_ID <> '7' ";
                 //   keyValueParameters.Add("serviceCategory", serviceCategory);

                }
            }


            where1 += @" and PEMP_EMP_CODE = :empCode ";

            keyValueParameters.Add("empCode", empCode);

            var query1 = @"


SELECT 
     ESS_LEAVE_REQ_DETAILS_ALL.*,
     TO_CHAR(ESS_LEAVE_REQ_DETAILS_ALL.ELRH_REQUESTED_DT,'DD-Mon-YYYY','nls_date_language=english') DT1,


     case 
        when est_service_id = 40 then -- incident request then show incident req status
            case when ELRH_REQ_STATUS_ID in ( 7,8) then 
                'Sent To Quality'
                else APPROVED_GROUP_NAME end
          when est_service_id = 41 then -- incident request then show incident req status
            case when ELRH_REQ_STATUS_ID in ( 7,8) then 
                'HOD Response Submitted'
                 when ELRH_REQ_STATUS_ID in 3 then 
                'Quality Sent to HOD/Director'    
                else APPROVED_GROUP_NAME end
                
        else 
        CASE WHEN ELRH_REQ_STATUS_ID = 11  THEN 
            --'Waiting For ' || Replace(INITCAP(AEG_DESCRIPTION),'Waiting For','') 
            GET_LEAVE_STATUS (ELRH_REQ_STATUS_ID)     
        ELSE  
            APPROVED_GROUP_NAME
        END  
    end 
CURR_STATUS_NAME_1,

    APPROVED_GROUP_NAME
        CURR_STATUS_NAME_2,
      case when est_service_id = 1 then 
        PLTM_LEAVE_TYPE_DESC
      when est_service_id = 5 then     
        ATT_REASON_DESC1 
      when est_service_id = 7 then
            ES_DESC
when est_service_id = 13 then
      AL_LETTER_NAME
      ELSE
            EST_SERVICE_DESC 
      END AS ESS_REQ_TYPE1,
    AWH_ROW_ID1 as AWH_ROW_ID
FROM 
    ESS_LEAVE_REQ_DETAILS_ALL, ESS_REQ_APPROVED_GROUP
    where
    to_char(LEAVE_REQ_ID) =to_char(UNIQUE_ID_1(+)) 
        AND to_char(LEAVE_REQ_ID) = to_char(UNIQUE_ID_2(+))
    " + where1 + " order by LEAVE_REQ_ID desc ";

             //   AppCode.LogError.WriteToLogFile("MyReqDataMyReqData", false, query1);
             if(ConfigurationManager.AppSettings["INCIDENT_ENABLED"] == "Y" )
            {
                query1 = GetMyReqQuery( empCode,  isAdmin,  serviceTypeId,  serviceCategory);
            }
            return db.execute_query_retun_datatable(query1, keyValueParameters);
        }

        private string GetMyReqQuery(string empCode, bool isAdmin, string serviceTypeId, string serviceCategory)
        {

            string where1 = "";

            Dictionary<string, object> keyValueParameters = new Dictionary<string, object>();
            //if (!isAdmin)
            //{
            //    where1 = @"      and exists( SELECT 1 FROM AUTH_WF_APPROVER_LOG where AWAL_WF_DET_ROW_ID = AWDD_ROW_ID and AWAL_COMP_CODE='01 and AWAL_EMP_CODE='" + empCode + @"') ";
            //}
            if (!string.IsNullOrEmpty(serviceTypeId))
            {
                where1 += " and EST_SERVICE_ID=:serviceTypeId ";
                keyValueParameters.Add("serviceTypeId", serviceTypeId);
            }
            else
            {
                // others selected, so dont show services
                if (serviceCategory != "")
                {
                    where1 += " and ES_CATEGORY=:serviceCategory ";
                keyValueParameters.Add("serviceCategory", serviceCategory);
                }
                else
                    where1 += " and EST_SERVICE_ID <> '7' ";
            }

            keyValueParameters.Add("empCode", empCode);

            where1 += @" and pemp. PEMP_EMP_CODE = :empCode ";
            var query1 = @"

 SELECT IIS_STATUS_DESC,ELR.ELRH_LEAVE_REQ_ID LEAVE_REQ_ID,
          --ELT_LEAVE_REQ_TRAN_ID TRAN_ID,
          PEMP.PEMP_EMP_CODE,
          PEMP.PEMP_EMP_NAME,
          ACD_COMP_NAME,
          PDPM_DEPARTMENT_DESC,
          PGDM_GRADE_DESC,
          ELRH_LEAVE_ID,
          ELR.ELRH_EMP_ID EMP_ID,
          ELR.ELRH_FROM_DT FROM_DT,
          ELRH_TO_DT TO_DT,
          ELR.ELRH_TOT_DAYS,
          ELR.ELRH_SUBJ SUBJ,
          ELR.ELRH_BODY LEAVE_BODY,
          ELR.ELRH_REQUESTED_DT,
          ELR.ELRH_REQ_STATUS_ID STATUS,
          --ELT_ACTION_BY ACTION_BY,
          --ELT_REQ_STATUS_ID REQ_STATUS,
          PEMP.PEMP_HR_LV_APPR,
          PEMP.PEMP_EMP_PHONE,
          PEMP.PEMP_EMAIL_ADDRESS,
          PEMP.PEMP_EMP_BRANCH_CODE,
          ELRH_SERVICE_TYPE,
          ELRH_VOUCHER_NO,
          EST_SERVICE_ID,
          EST_SERVICE_DESC,
          ELRH_LEAVE_REQ_ID,
          ELRH_REQ_STATUS_ID,
          ELS_STATUS_NAME CURR_STATUS_NAME,
          EMP.PEMP_EMP_NAME_AR PEMP_EMP_NAME_AR1,
          PBM_BRANCH_NAME_ARABIC PBM_BRANCH_NAME_ARABIC1,
          PDPM_DEPT_AR PDPM_DEPT_AR1,
          PDSM_DESIGNATION_DESC_AR PDSM_DESIGNATION_DESC_AR1,
          PGDM_GRADE_DESC_AR PGDM_GRADE_DESC_AR1,
          LR.ELR_SUBJ AS ATT_REASON_DESC1,
          LR.ELR_ATT_CORRECTION_REASON,
          TRM_REASON_DESC AS ATT_REASON_DESC,
          LR.ELR_SUBJ TRAINING_NAME,
          LR.ELR_BODY TRAINING_DESC,
          LR.ELR_TRAIN_PURPOSE,
          LR.ELR_TRAIN_GOAL,
          ES_CODE,
          ES_DESC,
          ES_DESC_AR,
          LR.ELR_SUBJ AS SERVICE_REQ_DESC,
--          LR.ELR_SUBJ AS PAYSLIP_REQ_DESC,
--          LR.ELR_BODY AS TRAINING_FEEDBACK_DESC,
          PLTM_LEAVE_TYPE_CODE,
          PLTM_LEAVE_TYPE_DESC,
     --     PLTM_LEAVE_ARABIC_DESC,
      --    LR.ELR_PARENT_TRAIN_REQ_ID,              -- , TRAIN.ELR_LEAVE_REQ_ID
          ELRH_NOTIFICATION_SENT,
       --   ELR_COMP_CODE,
       --   ELS_STATUS_NAME_AR,
          PBM_BRANCH_NAME,
          emp.PEMP_EMP_DEPTARTMENT_CODE,
          emp.PEMP_EMP_DESIGNATION_CODE,
          emp.PEMP_EMP_DESIGNATION_DESC,
         -- EST_SERVICE_DESC_AR,
          ELR_LEVEL_ID,
         -- ELR_LOAN_LOAN_TYPE,
         -- ELR_LOAN_AMOUNT,
         -- ELRH_CREATION_USER_ID,
        --  ES_CATEGORY,
          -- ES_CATEGORY,'' AL_LETTER_NAME
          ES_CATEGORY,
          AL_LETTER_NAME,
          TO_CHAR (ELR_APPROVED_DT, 'DD-MM-YYY') APP_DATE,
          TO_CHAR (ELR_APPROVED_DT, 'hh24:mi') APP_TIME,
          PEMP.PEMP_EMP_ACTIVE EMP_ACTIVE,
           TO_CHAR( ELRH_REQUESTED_DT,'DD-Mon-YYYY','nls_date_language=english') DT1,


     case 
        when est_service_id in ( 41, 40) then -- incident request then show incident req status
--            case when ELRH_REQ_STATUS_ID in ( 7,8) then 
--                'Sent To Quality'
--                else APPROVED_GROUP_NAME end
--          when est_service_id = 41 then -- incident request then show incident req status
--            case when ELRH_REQ_STATUS_ID in ( 7,8) then 
--                'HOD Response Submitted'
--                 when ELRH_REQ_STATUS_ID in 3 then 
--                'Quality Sent to HOD/Director'    
--                else APPROVED_GROUP_NAME end
                IIS_STATUS_DESC 
        else 
        CASE WHEN ELRH_REQ_STATUS_ID = 11  THEN 
            --'Waiting For ' || Replace(INITCAP(AEG_DESCRIPTION),'Waiting For','') 
            GET_LEAVE_STATUS (ELRH_REQ_STATUS_ID)     
        ELSE  
            APPROVED_GROUP_NAME
        END  
    end 
CURR_STATUS_NAME_1,

    APPROVED_GROUP_NAME
        CURR_STATUS_NAME_2,
      case when est_service_id = 1 then 
        PLTM_LEAVE_TYPE_DESC
      when est_service_id = 5 then     
        ELR_SUBJ 
      when est_service_id = 7 then
            ES_DESC
when est_service_id = 13 then
      AL_LETTER_NAME
      ELSE
            EST_SERVICE_DESC 
      END AS ESS_REQ_TYPE1,
    AWH_ROW_ID1 as AWH_ROW_ID, ELR_LOAN_REFERENCE
     --SELECT *
     FROM ESS_LEAVE_REQ_HEAD ELR,
          V_EMPLOYEE_DETAILS EMP,
          PPM_EMPLOYEE_DETAILS PEMP,
          ESS_SERVICE_TYPES,
          ESS_LEAVE_REQ LR,
          TAS_REASONS_MASTER,
          ESS_SERVICES,
          PPM_LEAVE_TYPE_MASTER,
          ESS_LEAVE_REQ_STAT_MST,
          AMM_LETTERS, ESS_REQ_APPROVED_GROUP, IMS_INCIDENT_REPORT, IMS_INCIDENT_STAUS 
    WHERE                       --ELR.ELRH_REQ_STATUS_ID(+) = SM.ELS_STATUS_ID
              -- and  ELT.ELT_REQ_STATUS_ID(+) = SM.ELS_STATUS_ID
              EMP.PEMP_EMP_CODE = ELR.ELRH_EMP_ID
          AND PEMP.PEMP_EMP_CODE = EMP.PEMP_EMP_CODE
          AND ELRH_SERVICE_TYPE = EST_SERVICE_ID
          AND ELR.ELRH_LEAVE_REQ_ID = LR.ELR_LEAVE_REQ_HEAD_ID
          AND LR.ELR_ATT_CORRECTION_REASON = TRM_REASON_CODE(+)
          AND ELR_COMP_CODE = TRM_COMP_CODE(+)
          AND LR.ELR_SERVICE_REQ_TYPE = ES_CODE(+)
          AND ELR_COMP_CODE = ES_COMP_CODE(+)
          AND PLTM_LEAVE_TYPE_CODE(+) = LR.ELR_LEAVE_ID
          AND ELR_COMP_CODE = ELRH_COMP_CODE
          AND ELR_COMP_CODE = EMP.PEMP_EMP_COMPANY_CODE
          AND ELR_COMP_CODE = PEMP.PEMP_EMP_COMPANY_CODE
          AND ELR_COMP_CODE = PLTM_COMP_CODE(+)
          AND ELS_STATUS_ID = ELRH_REQ_STATUS_ID
          AND AL_LETTER_CODE(+) = LR.ELR_LETTER_TYPE
           " + where1 + @"
    and to_char(ELRH_LEAVE_REQ_ID) =to_char(UNIQUE_ID_1(+)) 
        AND to_char(ELRH_LEAVE_REQ_ID) = to_char(UNIQUE_ID_2(+))
         AND IIR_INCIDENT_ID(+) = ELR_LOAN_REFERENCE
        AND IIS_STATUS_ID(+) = IIR_INCIDENT_STATUS  
   order by ELRH_LEAVE_REQ_ID desc
   
 ";

            return query1;
        }

        void GetEduDetail()
        {

            string str_select_empear_ded_detail1 = @"
        select * from ( SELECT 
        
            PEED_EDUCATION_SRL_NO,
            PEED_EDUCATION_INST_NAME, 
            PEED_EDUCATION_TYPE_FLAG, 
            PEED_EDUCATION_DEGREE,
            TO_CHAR(PEED_EDUCATION_JOIN_DATE, 'DD/MM/YYYY')  AS PEED_EDUCATION_JOIN_DATE , 
            TO_CHAR(PEED_EDUCATION_AWARD_DATE, 'DD/MM/YYYY') AS PEED_EDUCATION_AWARD_DATE, 
            PEED_EDUCATION_REMARKS,
            'N' as chk_delete
        from 
        PPT_EMP_EDUCATION_DETAILS
        WHERE PEED_EMP_CODE = '" + user.Emp_Code.Trim().Replace("'", "''") + @"'  ) 

";




            var dtEdu = db.execute_query_retun_datatable(str_select_empear_ded_detail1);
            var jsonEdu = "";
            if (dtEdu != null && dtEdu.Rows.Count > 0)
            {
                System.Web.Script.Serialization.JavaScriptSerializer serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>();
                Dictionary<string, object> row;
                foreach (DataRow dr in dtEdu.Rows)
                {
                    row = new Dictionary<string, object>();
                    foreach (DataColumn col in dtEdu.Columns)
                    {
                        row.Add(col.ColumnName, dr[col]);
                    }
                    rows.Add(row);
                }
                jsonEdu = serializer.Serialize(rows);
            }

            Response.Write(jsonEdu);


        }
        void GetRequestTypes()
        {

            var dt1 = db.execute_query_retun_datatable(@"
 SELECT
    EST_SERVICE_ID, EST_SERVICE_DESC
FROM ESS_SERVICE_TYPES where EST_IS_VISIBLE ='Y'  order by EST_SERVICE_DESC ");

            var JSONresult = JsonConvert.SerializeObject(dt1);
            Response.Write(JSONresult);

        }

        void GetRequestLeaveTypeOnly()
        {

            var dt1 = db.execute_query_retun_datatable(@"
 SELECT
     PLTM_LEAVE_TYPE_CODE,PLTM_LEAVE_TYPE_DESC from PPM_LEAVE_TYPE_MASTER  order by PLTM_LEAVE_TYPE_DESC,PLTM_SORT_ORDER  ");

            var JSONresult = JsonConvert.SerializeObject(dt1);
            Response.Write(JSONresult);

        }
        void GetCheckEmp()
        {

            var dt1 = db.execute_query_retun_datatable(@"
                select PEMP_EMP_CODE from PPM_EMPLOYEE_DETAILS
                where
                upper(PEMP_EMP_NAME) = upper('" + Request.QueryString["empname"].Replace("'", "''") + @"')
                or
                (PEMP_EMP_DATE_OF_BIRTH = TO_DATE('" + Request.QueryString["empdob"].ToArabicDate().ToString("dd/MM/yyyy") + @"', 'DD/MM/YYYY') AND  PEMP_EMP_JOIN_DATE = TO_DATE('" + Request.QueryString["empdoj"].ToArabicDate().ToString("dd/MM/yyyy") + "','DD/MM/YYYY'))");



            var JSONresult = JsonConvert.SerializeObject(dt1);
            Response.Write(JSONresult);

        }

        DataTable QueryNotificationsAll(string empCode, bool isAdmin)
        {
            return QueryNotifications(empCode, isAdmin, "");

        }

        void RejectAppraisal()
        {
            var empcode = Request.QueryString["employeecode"];
            string strquery1 = "", strquery2 = "";

            var reqHead = Request.QueryString["ReqId"];
            ESSWorkflowHeader ESSwORKFLOW = new ESSWorkflowHeader(reqHead.ToString(), reqHead.ToString());

            var strquery = "SELECT FUN_ESS_APVL_CMNT_STG(" + reqHead + ",'REJECT') FROM DUAL";
            var strcomm = objDB.execute_scalar(strquery);
            if (strcomm == "YES" && Request.Form["approveComment"].Trim() == "")
            {
                WriteErrorMessage("Error: Enter Comments.");
                return;
            }

            var comments = "Rejected:" + Request.Form["approveComment"];
            int count;
            //if (essLeaveRequestDA.Apply(SelectedManager, false, userDetail.Login_Company_Code))
            if (ESSwORKFLOW.UpdateWFStatus(false, comments))//essLeaveRequestDA.Approve(nextMngrCode, true, txtAppComments.Text, ERPCurrentUserInfo.GetCurrentUserInfo.LoginCompanyCode))
            {
                //CONDITION 1 IF CYCLE 1 IS REJECTED
                strquery1 = @"select count(*) from ess_leave_req where ELR_LEAVE_ID in ('CYCLE1') AND ELR_EMP_ID ='" + empcode + @"' AND ELR_REQ_STATUS_ID IN(5, 6)";
                count = db.execute_scalar(strquery1).ToInt();

                if (count >= 1)
                {
                    strquery2 = @"UPDATE ess_leave_req SET ELR_REQ_STATUS_ID = '11' WHERE ELR_LEAVE_ID in ('CYCLE2', 'CYCLE3') AND ELR_EMP_ID = '" + empcode + "'";
                    db.execute_query(strquery2);
                }

                // CONDITION 1 IF CYCLE 2 IS REJECTED

                strquery1 = @"select count(*) from ess_leave_req where ELR_LEAVE_ID in ('CYCLE2') AND ELR_EMP_ID = '" + empcode + @"' AND ELR_REQ_STATUS_ID IN(5, 6)";
                count = db.execute_scalar(strquery1).ToInt();
                if (count >= 1)
                {
                    strquery2 = @"UPDATE ess_leave_req SET ELR_REQ_STATUS_ID = '11' WHERE ELR_LEAVE_ID in ('CYCLE3') AND ELR_EMP_ID = '" + empcode + "'";
                    db.execute_query(strquery2);
                }

                WriteSuccessMessage("The record is rejected.");

            }
            else
                WriteErrorMessage("Error: Unable to reject the record.");

        }

        public int filecount = 0;
        void SaveDocAppraisal(string serviceType)
        {
            var reqHead = Request.QueryString["ReqId"];
            var empcode = Request.QueryString["employeecode"];
            // string oldFolderPath = HttpContext.Current.Request.MapPath("~/ESS/Docs/") + "/" + leaveReqHeader.ELR_EMP_ID + "/"
            //+ "/" + (int)leaveReqHeader.ELR_SERVICE_TYPE + "/" + HttpContext.Current.Session.SessionID;
            try
            {
                Directory.CreateDirectory(Server.MapPath("~/ESS/Docs"));
            }
            catch { }
            try
            {
                Directory.CreateDirectory(Server.MapPath("~/ESS/Docs/" + empcode));
            }
            catch { }

            try
            {
                Directory.CreateDirectory(Server.MapPath("~/ESS/Docs/" + empcode + "/" + serviceType));
            }
            catch { }


            try
            {
                Directory.CreateDirectory(Server.MapPath("~/ESS/Docs/" + empcode + "/" + serviceType + "/" + reqHead));

            }
            catch { }

            HttpPostedFile file = HttpContext.Current.Request.Files["fileInput"];

            try
            {

                if (file != null && file.ContentLength > 0)
                {
                    string fname = Path.GetExtension(file.FileName.Replace("'", ""));
                    filecount = 1;
                    file.SaveAs(Server.MapPath("~/ESS/Docs/" + empcode + "/" + serviceType + "/" + reqHead + "/" + file.FileName.Replace("'", "")));
                     
                }

             }
            catch (Exception e1)
            {
                AppCode.LogError.WriteToLogFile(e1.ToString(), "Error while saving appraisal document", "");
            }
            HttpPostedFile file2 = Request.Files["fileInputArabic"];

            //check file was submitted
            if (file2 != null && file2.ContentLength > 0)
            {
                string fname = Path.GetExtension(file2.FileName);
                file2.SaveAs(Server.MapPath("~/ESS/Docs/" + Request.Form["employeecode"] + "/" + serviceType + "/" + reqHead + "/" + file2.FileName));
             }
        }

        void Appraisal2()
        {
            var Services = Request.QueryString["Service"];
            var reqHead = Request.QueryString["ReqId"];
            ESSWorkflowHeader ESSwORKFLOW = new ESSWorkflowHeader(reqHead.ToString(), reqHead.ToString());

            var strquery = "SELECT FUN_ESS_APVL_CMNT_STG(" + reqHead + ",'APPROVE') FROM DUAL";
            var strcomm = objDB.execute_scalar(strquery);
            if (strcomm == "YES" && Request.Form["approveComment"].Trim() == "")
            {
                WriteErrorMessage("Error: Enter Comments.");
                return;
            }
            //var comments =  Request.Form["approveComment"];
            var comments = Request.Form["approveComment"];

            var empid = Request.QueryString["employeecode"];
            var recommend = Request.QueryString["valdt"];
            var orgCost = "";
            if (comments.Length > 1400)
            {
                WriteErrorMessage("Error: Comments cant exceed 1400 characters.");
                return;
            }
            if (!string.IsNullOrEmpty(reqHead))
            {

                objDB.execute_query(@"
                UPDATE ESS_LEAVE_REQ
                SET    
                       ELR_TRAIN_PURPOSE    = '" + recommend + @"'
                WHERE  ELR_LEAVE_REQ_HEAD_ID      = '" + reqHead + @"'
                ");

            }

            if (!string.IsNullOrEmpty(Request["EARD_ORG_SOFTWARE_COST"]))
            {
                orgCost = Request["EARD_ORG_SOFTWARE_COST"].ToString();

                objDB.execute_query(@"
                UPDATE ESS_AER_REQUEST_DET
                SET    
                       EARD_ORG_SOFTWARE_COST    = '" + orgCost + @"'
                WHERE  EARD_REQUEST_HEAD_ID      = '" + reqHead + @"'
                ");

            }

            var strdata = db.execute_query_retun_datatable(@"SELECT 
    COUNT(1) CNT FROM AUTH_WF_DOC_DETAIL,AUTH_WF_HEADER
where     AWDD_WF_HEADER_ROW_ID =AWH_ROW_ID
    and AWH_UNIQUE_ID1= '" + reqHead + @"'
    and  AWH_UNIQUE_ID2 = '" + reqHead + @"'
    and AWDD_STATUS in ('A','R')");

            if (strdata != null && strdata.Rows[0]["CNT"].ToString() == "0")
            {
                var filesAttached = false;
                HttpPostedFile file = HttpContext.Current.Request.Files["fileInput"];


                if (file != null && file.ContentLength > 0)
                {
                    filesAttached = true;
                }
                var folder = HttpContext.Current.Request.MapPath("~/ESS/Docs/") + "/" + empid + "/"
             + "/" + Services + "/" + reqHead;

                try
                {
                    if (!filesAttached)
                    {
                        var files = Directory.GetFiles(folder);
                        filesAttached = (files != null && files.Length > 0);
                    }
                }
                catch
                {

                }

                if (!filesAttached)
                {
                    WriteErrorMessage("Error: attachment is mandatory.");
                    return;
                }


            }
            // validate 

            // adde by bala on 13-06-2023 , approver cant approve again and again by clicking same approve button
            if (!IsApproverFoundInWorkflow(reqHead.ToInt()))
                return;

            if (ESSwORKFLOW.UpdateWFStatus(true, comments))//essLeaveRequestDA.Approve(nextMngrCode, true, txtAppComments.Text, ERPCurrentUserInfo.GetCurrentUserInfo.LoginCompanyCode))
            {
                EssProbationNewDA essProbationNewDA = new EssProbationNewDA();

                if (essProbationNewDA.Populate(reqHead.ToInt()))
                {
                    // update from controls
                    foreach (var item in essProbationNewDA.DTO.ProbationLogs)
                    {


                        item.EPL_KPI_DESC = Request[item.EPL_KPI_CODE + "Desc"];
                        item.EPL_SCORE = (decimal)Request[item.EPL_KPI_CODE + "Rate"].ToDouble();
                        item.EPL_TARGET_DATE = Request[item.EPL_KPI_CODE + "Date"].ToArabicDateNull();

                    }
                    essProbationNewDA.Update();
                }

                WriteSuccessMessage("The record is approved.");
            }
            else
                WriteErrorMessage("Error: Unable to approve the record.");


        }
        void Appraisal()
        {
            var Services = Request.QueryString["Service"];
            var reqHead = Request.QueryString["ReqId"];
            ESSWorkflowHeader ESSwORKFLOW = new ESSWorkflowHeader(reqHead.ToString(), reqHead.ToString());

            var strquery = "SELECT FUN_ESS_APVL_CMNT_STG(" + reqHead + ",'APPROVE') FROM DUAL";
            var strcomm = objDB.execute_scalar(strquery);
            if (strcomm == "YES" && Request.Form["approveComment"].Trim() == "")
            {
                WriteErrorMessage("Error: Enter Comments.");
                return;
            }
            //var comments =  Request.Form["approveComment"];
            var comments = Request.Form["approveComment"];

            var empid = Request.QueryString["employeecode"];
            var recommend = Request.QueryString["valdt"];
            var orgCost = "";
            if (comments.Length > 1400)
            {
                WriteErrorMessage("Error: Comments cant exceed 1400 characters.");
                return;
            }
            if (!string.IsNullOrEmpty(reqHead))
            {

                objDB.execute_query(@"
                UPDATE ESS_LEAVE_REQ
                SET    
                       ELR_TRAIN_PURPOSE    = '" + recommend + @"'
                WHERE  ELR_LEAVE_REQ_HEAD_ID      = '" + reqHead + @"'
                ");

            }

            if (!string.IsNullOrEmpty(Request["EARD_ORG_SOFTWARE_COST"]))
            {
                orgCost = Request["EARD_ORG_SOFTWARE_COST"].ToString();

                objDB.execute_query(@"
                UPDATE ESS_AER_REQUEST_DET
                SET    
                       EARD_ORG_SOFTWARE_COST    = '" + orgCost + @"'
                WHERE  EARD_REQUEST_HEAD_ID      = '" + reqHead + @"'
                ");

            }

            var strdata = db.execute_query_retun_datatable(@"SELECT 
    COUNT(1) CNT FROM AUTH_WF_DOC_DETAIL,AUTH_WF_HEADER
where     AWDD_WF_HEADER_ROW_ID =AWH_ROW_ID
    and AWH_UNIQUE_ID1= '" + reqHead + @"'
    and  AWH_UNIQUE_ID2 = '" + reqHead + @"'
    and AWDD_STATUS in ('A','R')");

            if (strdata != null && strdata.Rows[0]["CNT"].ToString() == "0")
            {
                var filesAttached = false;
                HttpPostedFile file = HttpContext.Current.Request.Files["fileInput"];


                if (file != null && file.ContentLength > 0)
                {
                    filesAttached = true;
                }
                var folder = HttpContext.Current.Request.MapPath("~/ESS/Docs/") + "/" + empid + "/"
             + "/" + Services + "/" + reqHead;

                try
                {
                    if (!filesAttached)
                    {
                        var files = Directory.GetFiles(folder);
                        filesAttached = (files != null && files.Length > 0);
                    }
                }
                catch
                {

                }

                if (!filesAttached)
                {
                    WriteErrorMessage("Error: attachment is mandatory.");
                    return;
                }


            }

            // adde by bala on 13-06-2023 , approver cant approve again and again by clicking same approve button
            if (!IsApproverFoundInWorkflow(reqHead.ToInt()))
                return;

            if (ESSwORKFLOW.UpdateWFStatus(true, comments))//essLeaveRequestDA.Approve(nextMngrCode, true, txtAppComments.Text, ERPCurrentUserInfo.GetCurrentUserInfo.LoginCompanyCode))
            {
                WriteSuccessMessage("The record is approved.");
            }
            else
                WriteErrorMessage("Error: Unable to approve the record.");


        }

        void APPROVEDBYMYSELF(string empcode)
        {
            var fromDate1 = Request["FromDate"].ToArabicDate();
            var toDate1 = Request["ToDate"].ToArabicDate();
            var service = Request["Service"];
            DBAccess ObjDb = new DBAccess();
            string strquery = @"

SELECT DISTINCT ELR_LEAVE_REQ_HEAD_ID,
    ELR_EMP_ID, ELR_LEAVE_ID, to_char(ELR_FROM_DT,'dd/mm/yyyy') ELR_FROM_DT1, 
    to_char(ELR_TO_DT,'dd/mm/yyyy') ELR_TO_DT1, ELR_TOT_DAYS, ELR_BODY, 
    ELR_REQ_STATUS_ID,EST_SERVICE_ID, EST_SERVICE_DESC, PLTM_LEAVE_TYPE_DESC,
    EMPID, ENAME,to_char(ELR_REQUESTED_DT,'dd/mm/yyyy')REQ_DATE, 
    BRANCH_NAME, DEPT_NAME, DIV_NAME, DESIGNATION_DESC,APPROVED_GROUP_NAME
    
FROM 
    ESS_LEAVE_REQ,ESS_WF_DETAIL,ESS_SERVICE_TYPES,PPM_LEAVE_TYPE_MASTER,
    VU_EMP_DET,ESS_REQ_APPROVED_GROUP
WHERE
     EMPID=ELR_EMP_ID   AND GROUP_APRROVED_EMP_CODE ='" + empcode + @"' 
     AND ELR_REQ_STATUS_ID not in( 1,2, 5, 6) AND ELR_LEAVE_ID = PLTM_LEAVE_TYPE_CODE(+)
    AND ELR_SERVICE_TYPE= EST_SERVICE_ID(+)";

            if (service.ToInt() > 0)
            {
                strquery += " AND EST_SERVICE_ID=" + service.ToInt() + "";
            }
            strquery += @"AND AWH_UNIQUE_ID1 =TO_CHAR(ELR_LEAVE_REQ_HEAD_ID)
     AND UNIQUE_ID_1 =  TO_CHAR(ELR_LEAVE_REQ_HEAD_ID)
     AND UNIQUE_ID_2 = TO_CHAR(ELR_LEAVE_REQ_HEAD_ID)    
    
and (( Trunc(ELR_FROM_DT) BETWEEN to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') OR
        trunc(ELR_TO_DT) BETWEEN to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy')) OR 
          ( trunc(ELR_FROM_DT) < to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"','dd/mm/yyyy') < trunc(ELR_TO_DT) ))
    ORDER BY ELR_LEAVE_REQ_HEAD_ID desc ";

            LogError.WriteToLogFile("APPROVED byme", false, strquery);
            DataTable dt1 = objDB.execute_query_retun_datatable(strquery);
            if (dt1 != null && dt1.Rows.Count > 0)
            {
                var JSONresult = JsonConvert.SerializeObject(dt1);
                Response.Write(JSONresult);
            }
            else
            {
                ResponseWrite("");
            }
        }
        private void GetEmployeeDetailsManger()
        {
            var empcode = Request.QueryString["EmpCode"];
            ERPCurrentUserInfo eRPCurrentUserInfo = ERPCurrentUserInfo.GetCurrentUserInfo;

            var query = "";

            if (eRPCurrentUserInfo.IsAdministrator)
            {
                query = @"
 
             SELECT  
                PEMP_EMP_CODE  ,  PEMP_EMP_NAME  
            FROM PPM_EMPLOYEE_DETAILS  
                WHERE 
                      PEMP_EMP_ACTIVE='Y' ";
            }
            else
            {


                List<string> arrayQueries = new List<string>();
                //if (eRPCurrentUserInfo.IsBranchAdministrator)
                //    arrayQueries.Add(" select pemp_emp_code from ppm_employee_Details where   PEMP_EMP_BRANCH_CODE= '" + eRPCurrentUserInfo.BranchCode + @"'  ");
                //if (eRPCurrentUserInfo.IsDepartmentHead)
                //    arrayQueries.Add(" select pemp_emp_code from ppm_employee_Details where   PEMP_EMP_DEPTARTMENT_CODE= '" + eRPCurrentUserInfo.DepartmentCode + @"'  ");

                arrayQueries.Add(" select pemp_emp_code from ppm_employee_Details where   pemp_reporting_to= '" + eRPCurrentUserInfo.EMP_ID + @"'  ");
                arrayQueries.Add(@"SELECT EAFO_EMP_CODE  FROM ESS_APPLY_FOR_OTHERS where EAFO_MANAGER_CODE ='" + eRPCurrentUserInfo.EMP_ID + @"' ");

                var supervisorFilter = @" and ( exists ( select 1 from (  Select pemp_emp_code ecode1 from( " + string.Join(" Union All ", arrayQueries) + " ) ) where ecode1= PEMP_EMP_CODE )  or PEMP_EMP_CODE='" + eRPCurrentUserInfo.EmpCode + @"')";

                query = @"
 
             SELECT  
                PEMP_EMP_CODE  ,  PEMP_EMP_NAME  
            FROM PPM_EMPLOYEE_DETAILS  
                WHERE   PEMP_EMP_ACTIVE='Y'";
                query += supervisorFilter + "";
            }
            query += " ORDER BY TO_NUMBER(REGEXP_SUBSTR(PEMP_EMP_CODE, '^[[:digit:]]*')) ";

            log_error.write_to_log_file("To view others", query, "true");

            var dt1 = db.execute_query_retun_datatable(query);

            var JSONresult = JsonConvert.SerializeObject(dt1);
            Response.Write(JSONresult);


        }


        void ATTENDANCEVIEW(string empCode)
        {
            var Services = Request.Form["Service"];
            var fromDate1 = Request.QueryString["FromDate"].ToArabicDate(); ;
            var toDate1 = Request.QueryString["ToDate"].ToArabicDate();

           
            string strquery = @"SELECT ROW_NUMBER() OVER(ORDER BY EMPID)SNO ,EMPID,ENAME,BRANCH_NAME,DEPT_NAME,DESIG_NAME,TO_CHAR(ATT_DATE,'DD-MM-YYYY')ATTDATE,TO_CHAR(ATT_DATE,'DAY', 'NLS_DATE_LANGUAGE=ENGLISH')DAY,TO_CHAR(IN1_A,'HH24:MI')IN1_A,TO_CHAR(OUT1_A,'HH24:MI')OUT1_A,TO_CHAR(IN2_A,'HH24:MI')IN2_A,TO_CHAR(OUT2_A,'HH24:MI')OUT2_A,
            TO_CHAR(IN3_A,'HH24:MI')IN3_A,TO_CHAR(OUT3_A,'HH24:MI')OUT3_A,PRESENT_ABSENT";

        
                strquery += @" FROM V_ATT_TRAN VAT  WHERE    
           NVL(PEMP_EMP_ACTIVE,'Y') ='Y' AND NVL(PEMP_NO_TAS,'Y') = 'Y' AND  EMPID='" + empCode + @"'               
           and ATT_DATE BETWEEN TO_DATE('" + fromDate1.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy') ";
          
            
            strquery += " ORDER BY SNO,ATT_DATE desc";


            log_error.write_to_log_file("View Attendance", "true", strquery);
            DataTable dt1 = objDB.execute_query_retun_datatable(strquery);
            if (dt1 != null && dt1.Rows.Count > 0)
            {
                var JSONresult = JsonConvert.SerializeObject(dt1);
                Response.Write(JSONresult);
            }
            else
            {
                Response.Write("{}");
            }


        }

        void ATTENDANCEVIEW_OTHERS(string empCode)
        {
            var Services = Request.Form["Service"];
            var fromDate1 = Request.QueryString["FromDate"].ToArabicDate(); ;
            var toDate1 = Request.QueryString["ToDate"].ToArabicDate();

         
            string strquery = @"SELECT ROW_NUMBER() OVER(ORDER BY EMPID)SNO ,EMPID,ENAME,BRANCH_NAME,DEPT_NAME,DESIG_NAME,TO_CHAR(ATT_DATE,'DD-MM-YYYY')ATTDATE,TO_CHAR(ATT_DATE,'DAY')DAY,TO_CHAR(IN1_A,'HH24:MI')IN1_A,TO_CHAR(OUT1_A,'HH24:MI')OUT1_A,TO_CHAR(IN2_A,'HH24:MI')IN2_A,TO_CHAR(OUT2_A,'HH24:MI')OUT2_A,
TO_CHAR(IN3_A,'HH24:MI')IN3_A,TO_CHAR(OUT3_A,'HH24:MI')OUT3_A,PRESENT_ABSENT   FROM V_ATT_TRAN VAT WHERE 1=1
        AND  NVL(PEMP_EMP_ACTIVE,'Y') ='Y' AND NVL(PEMP_NO_TAS,'Y') = 'Y' AND  EMPID='" + empCode + @"'               
           and ATT_DATE BETWEEN TO_DATE('" + fromDate1.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy')";
            strquery += " ORDER BY SNO,ATT_DATE desc";

            // //AND ATT_DATE BETWEEN " + str_from_date + " AND " + str_to_date + "";
            /*OR

        TO_DT BETWEEN to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy')) OR
            (FROM_DT < to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy') < TO_DT))
ORDER BY ELRH_REQUESTED_DT DESC";*/

            // and EST_SERVICE_ID = " + Services + @"

            DataTable dt1 = objDB.execute_query_retun_datatable(strquery);
            if (dt1 != null && dt1.Rows.Count > 0)
            {
                var JSONresult = JsonConvert.SerializeObject(dt1);
                Response.Write(JSONresult);
            }
            else
            {
                Response.Write("");
            }


        }
        void ATTENDANCPUNCHEVIEW(string empCode)
        {
            var Services = Request.Form["Service"];
            var fromDate1 = Request.QueryString["FromDate"].ToArabicDate(); ;
            var toDate1 = Request.QueryString["ToDate"].ToArabicDate();

    
            string strquery = @"SELECT ROW_NUMBER() OVER(ORDER BY EMPID)SNO ,EMPID,ENAME,BRANCH_NAME,DEPT_NAME,DESIG_CODE,GRP_NAME,TO_CHAR(TAL_PUNCH_TIME,'DD-MM-YYYY')ATTDATE,TO_CHAR(TAL_PUNCH_TIME,'DAY')DAY,
            TO_CHAR(TAL_PUNCH_TIME,'HH24:MI')PUNCHTIME ,TAL_PUNCH_TYPE_DESC,LOCATION_URL FROM  V_RAW_PUNCH_NEW WHERE    NVL(PEMP_EMP_ACTIVE,'Y') ='Y' AND NVL(PEMP_NO_TAS,'Y') = 'Y'  AND  EMPID='" + empCode + @"'               
           and TAL_PUNCH_TIME BETWEEN TO_DATE('" + fromDate1.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy')";

            strquery += " ORDER BY SNO,BRANCH_CODE, DEPT_CODE,  EMPID ,TAL_PUNCH_TIME ";

            log_error.write_to_log_file("View Attendance Punch", "true", strquery);
            DataTable dt1 = objDB.execute_query_retun_datatable(strquery);
            if (dt1 != null && dt1.Rows.Count > 0)
            {
                var JSONresult = JsonConvert.SerializeObject(dt1);
                Response.Write(JSONresult);
            }
            else
            {
                Response.Write("{}");
            }


        }
        void APPRAISALHISTORY(string empCode)
        {
            var Services = Request.Form["Service"];
            var fromDate1 = Request.QueryString["FromDate"].ToArabicDate(); ;
            var toDate1 = Request.QueryString["ToDate"].ToArabicDate();

            string strquery = @"SELECT 
     ESS_LEAVE_REQ_DETAILS_ALL.*,to_char(ELRH_REQUESTED_DT,'dd/mm/yyyy')REQ_DATE,
     TO_CHAR(ESS_LEAVE_REQ_DETAILS_ALL.ELRH_REQUESTED_DT,'DD-Mon-YYYY','nls_date_language=english') DT1,       
          APPROVED_GROUP_NAME 
          CURR_STATUS_NAME_1,to_char(TO_DT,'dd/mm/yyyy') PROB_CYCLE_ENDDT,APP_CNT, CONCAT(CONCAT(APP_DATE,' '),APP_TIME)APP_DATE,APP_TIME
   
FROM 
    ESS_LEAVE_REQ_DETAILS_ALL,
        ESS_REQ_APPROVED_GROUP,
      ( select AWDD_WF_HEADER_ROW_ID WF_HEADER_ID, SUM(CASE WHEN wd2.AWDD_STATUS in ('A','R') THEN 1 ELSE 0 END) AS APP_CNT
  from AUTH_WF_DOC_DETAIL wd2  
                    GROUP BY AWDD_WF_HEADER_ROW_ID)
    where
             UNIQUE_ID_1 =  TO_CHAR(LEAVE_REQ_ID)
                    AND UNIQUE_ID_2 = TO_CHAR(LEAVE_REQ_ID) 
    and WF_HEADER_ID(+) = AWH_ROW_ID1
    AND EST_SERVICE_ID in(20,21,22)     
            AND EMP_ID = '" + empCode + @"'
             and((FROM_DT BETWEEN to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy') OR

           TO_DT BETWEEN to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy')) OR
               (FROM_DT < to_date('" + fromDate1.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy') AND to_date('" + toDate1.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy') < TO_DT))
ORDER BY LEAVE_REQ_ID DESC";

            // and EST_SERVICE_ID = " + Services + @"

            DataTable dt1 = objDB.execute_query_retun_datatable(strquery);
            if (dt1 != null && dt1.Rows.Count > 0)
            {
                var JSONresult = JsonConvert.SerializeObject(dt1);
                Response.Write(JSONresult);
            }
            else
            {
                Response.Write("{}");
            }


        }
        void APPRAISALAPPROVEDBYME(string empCode)
        {
            var from = Request["FromDate"].ToArabicDate();
            var to = Request["ToDate"].ToArabicDate();
            var Services = Request.Form["Service"];
            DBAccess ObjDb = new DBAccess();
            // var streval = objDB.execute_scalar("SELECT TAP_PARAVALUE FROM  TAS_ATT_PARAM WHERE  TAP_PARANAME='COMP_OFF_LEAVE_CODE' ");
            string strquery = @"

SELECT 
     ESS_LEAVE_REQ_DETAILS_ALL.*,to_char(ELRH_REQUESTED_DT,'dd/mm/yyyy')REQ_DATE,
      
          APPROVED_GROUP_NAME 
          CURR_STATUS_NAME_1,to_char(TO_DT,'dd/mm/yyyy') PROB_CYCLE_ENDDT, CONCAT(CONCAT(APP_DATE,' '),APP_TIME)APP_DATE,APP_TIME
    
   
FROM 
     ESS_LEAVE_REQ_DETAILS_ALL,
        ESS_REQ_APPROVED_GROUP,
      ( select AWDD_WF_HEADER_ROW_ID WF_HEADER_ID, SUM(CASE WHEN wd2.AWDD_STATUS in ('A','R') THEN 1 ELSE 0 END) AS APP_CNT
  from AUTH_WF_DOC_DETAIL wd2  
                    GROUP BY AWDD_WF_HEADER_ROW_ID)
    where
             UNIQUE_ID_1 =  TO_CHAR(LEAVE_REQ_ID)
                    AND UNIQUE_ID_2 = TO_CHAR(LEAVE_REQ_ID) 
    and WF_HEADER_ID(+) = AWH_ROW_ID1
    AND EST_SERVICE_ID in(20,21,22)           

and exists ( 
    SELECT 1 FROM AUTH_WF_HEADER, AUTH_WF_DOC_DETAIL
              WHERE     
                    AWDD_WF_HEADER_ROW_ID = AWH_ROW_ID
                    AND AWDD_EMP_APPROVED_BY = '" + empCode + @"'
                    AND AWH_UNIQUE_ID1 =  TO_CHAR(LEAVE_REQ_ID)
                    AND AWH_UNIQUE_ID2 = TO_CHAR(LEAVE_REQ_ID)
    )

             and((FROM_DT BETWEEN to_date('" + from.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy') AND to_date('" + to.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy') OR
            TO_DT BETWEEN to_date('" + from.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy') AND to_date('" + to.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy')) OR
                (FROM_DT < to_date('" + from.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy') AND to_date('" + to.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy') < TO_DT))
    ORDER BY LEAVE_REQ_ID DESC";

            DataTable dt1 = objDB.execute_query_retun_datatable(strquery);
            if (dt1 != null && dt1.Rows.Count > 0)
            {
                var fname = "";
                dt1.Columns.Add("FileName");
                foreach (DataRow dr in dt1.Rows)
                {
                    if (dr["LEAVE_REQ_ID"].ToString() != "")
                    {
                        fname = Getfilinfo("21", dr["PEMP_EMP_CODE"].ToString(), dr["LEAVE_REQ_ID"].ToString());
                        dr["FileName"] = fname;
                    }

                }
                var JSONresult = JsonConvert.SerializeObject(dt1);
                Response.Write(JSONresult);
            }
            else
            {
                Response.Write("{}");
            }
        }
        void SaveDocs(string serviceType)
        {

            // string oldFolderPath = HttpContext.Current.Request.MapPath("~/ESS/Docs/") + "/" + leaveReqHeader.ELR_EMP_ID + "/"
            //+ "/" + (int)leaveReqHeader.ELR_SERVICE_TYPE + "/" + HttpContext.Current.Session.SessionID;
            try
            {
                Directory.CreateDirectory(Server.MapPath("~/ESS/Docs"));
            }
            catch { }
            try
            {
                Directory.CreateDirectory(Server.MapPath("~/ESS/Docs/" + Request.Form["EMPCode"]));
            }
            catch { }

            try
            {
                Directory.CreateDirectory(Server.MapPath("~/ESS/Docs/" + Request.Form["EMPCode"] + "/" + serviceType));
            }
            catch { }


            try
            {
                Directory.CreateDirectory(Server.MapPath("~/ESS/Docs/" + Request.Form["EMPCode"] + "/" + serviceType + "/" + Session.SessionID));

            }
            catch { }

            HttpPostedFile file = HttpContext.Current.Request.Files["fileInput"];

            try
            {

                if (file != null && file.ContentLength > 0)
                {
                    string fname = Path.GetExtension(file.FileName);

                    //file.SaveAs(Server.MapPath("~/ESS/Docs/" + Request.Form["EMPCode"] + "/" + serviceType + "/" + file.FileName));

                    file.SaveAs(Server.MapPath("~/ESS/Docs/" + Request.Form["EMPCode"] + "/" + serviceType + "/" + Session.SessionID + "/" + file.FileName.Replace("'", "")));
                    //AppCode.LogError.WriteToLogFile(fname, "extension is", fname);
                    //file2.SaveAs(Server.MapPath(Path.Combine("~/App_Data/", fname)));
                }

                //AppCode.LogError.WriteToLogFile("Before save", "", "");


                // file.SaveAs(Server.MapPath("~/ESS/Docs/" + Request.Form["EMPCode"] + "/" + serviceType + "/" + Session.SessionID + "/FileEn" + fname));
            }
            catch (Exception e1)
            {
                AppCode.LogError.WriteToLogFile(e1.ToString(), "", "");
            }
            HttpPostedFile file2 = Request.Files["fileInputArabic"];

            //check file was submitted
            if (file2 != null && file2.ContentLength > 0)
            {
                string fname = Path.GetExtension(file2.FileName);
                file2.SaveAs(Server.MapPath("~/ESS/Docs/" + Request.Form["EMPCode"] + "/" + serviceType + "/" + Session.SessionID + "/" + file2.FileName));
                //file2.SaveAs(Server.MapPath(Path.Combine("~/App_Data/", fname)));
            }






        }


        void PendingDocs(string empCode,string requestid,string serviceType)
        {

            // string oldFolderPath = HttpContext.Current.Request.MapPath("~/ESS/Docs/") + "/" + leaveReqHeader.ELR_EMP_ID + "/"
            //+ "/" + (int)leaveReqHeader.ELR_SERVICE_TYPE + "/" + HttpContext.Current.Session.SessionID;
            try
            {
                Directory.CreateDirectory(Server.MapPath("~/ESS/Docs"));
            }
            catch { }
            try
            {
                Directory.CreateDirectory(Server.MapPath("~/ESS/Docs/" + empCode));
            }
            catch { }

            try
            {
                Directory.CreateDirectory(Server.MapPath("~/ESS/Docs/" + empCode + "/" + serviceType));      
            }
            catch { }

            try
            {
                Directory.CreateDirectory(Server.MapPath("~/ESS/Docs/" + empCode + "/" + serviceType + "/" + requestid));
                if (Directory.Exists(Server.MapPath("~/ESS/Docs/" + empCode + "/" + serviceType+ "/" + requestid)))
                {
                    string[] filePaths = Directory.GetFiles(Server.MapPath("~/ESS/Docs/" + empCode + "/" + serviceType+"/" + requestid));

                    foreach (var fi in filePaths)
                    {
                        File.Delete(fi);
                    }
                }
            }
            catch { }

            HttpPostedFile file = HttpContext.Current.Request.Files["fileInput"];

            try
            {

                if (file != null && file.ContentLength > 0)
                {
                   
                    string fname = Path.GetExtension(file.FileName.Replace("'", ""));
                    var filePathToSave = Server.MapPath("~/ESS/Docs/" + empCode + "/" + serviceType + "/" + requestid + "/" + file.FileName.Replace("'", ""));
                    AppCode.LogError.WriteToLogFile(filePathToSave.ToString(), "", "");


                    file.SaveAs(filePathToSave);
  
                }

              
            }
            catch (Exception e1)
            {
                AppCode.LogError.WriteToLogFile(e1.ToString(), "", "");
            }
          



        }


        void SaveDocs(string serviceType, string empCode)
        {

            // string oldFolderPath = HttpContext.Current.Request.MapPath("~/ESS/Docs/") + "/" + leaveReqHeader.ELR_EMP_ID + "/"
            //+ "/" + (int)leaveReqHeader.ELR_SERVICE_TYPE + "/" + HttpContext.Current.Session.SessionID;
            try
            {
                Directory.CreateDirectory(Server.MapPath("~/ESS/Docs"));
            }
            catch { }
            try
            {
                Directory.CreateDirectory(Server.MapPath("~/ESS/Docs/" + empCode));
            }
            catch { }

            try
            {
                Directory.CreateDirectory(Server.MapPath("~/ESS/Docs/" + empCode + "/" + serviceType));
            }
            catch { }


            try
            {
                Directory.CreateDirectory(Server.MapPath("~/ESS/Docs/" + empCode + "/" + serviceType + "/" + Session.SessionID));

            }
            catch { }

            HttpPostedFile file = HttpContext.Current.Request.Files["fileInput"];

            try
            {

                if (file != null && file.ContentLength > 0)
                {
                    string fname = Path.GetExtension(file.FileName.Replace("'", ""));

                    //file.SaveAs(Server.MapPath("~/ESS/Docs/" + Request.Form["EMPCode"] + "/" + serviceType + "/" + file.FileName));

                    file.SaveAs(Server.MapPath("~/ESS/Docs/" + empCode + "/" + serviceType + "/" + Session.SessionID + "/" + file.FileName.Replace("'", "")));
                    //AppCode.LogError.WriteToLogFile(fname, "extension is", fname);
                    //file2.SaveAs(Server.MapPath(Path.Combine("~/App_Data/", fname)));
                }

                //AppCode.LogError.WriteToLogFile("Before save", "", "");


                // file.SaveAs(Server.MapPath("~/ESS/Docs/" + Request.Form["EMPCode"] + "/" + serviceType + "/" + Session.SessionID + "/FileEn" + fname));
            }
            catch (Exception e1)
            {
                AppCode.LogError.WriteToLogFile(e1.ToString(), "", "");
            }
            HttpPostedFile file2 = Request.Files["fileInputArabic"];

            //check file was submitted
            if (file2 != null && file2.ContentLength > 0)
            {
                string fname = Path.GetExtension(file2.FileName);
                file2.SaveAs(Server.MapPath("~/ESS/Docs/" + Request.Form["EMPCode"] + "/" + serviceType + "/" + Session.SessionID + "/" + file2.FileName));
                //file2.SaveAs(Server.MapPath(Path.Combine("~/App_Data/", fname)));
            }






        }





        void DeleteFiles(string serviceType)
        {
            try
            {
                Directory.Delete(Server.MapPath("~/ESS/Docs/" + Request.Form["EMPCode"] + "/" + serviceType + "/" + Session.SessionID));
            }
            catch { }
        }
        void DeleteFiles(string serviceType, string Empcod)
        {
            try
            {
                Directory.Delete(Server.MapPath("~/ESS/Docs/" + Empcod + "/" + serviceType + "/" + Session.SessionID));
            }
            catch { }
        }
        //[System.Web.Services.WebMethod]
        //public static string getchangetime(string fromdate)
        //{
        //    return string.Format("FromDate :{0}", fromdate, Environment.NewLine);
        //}

        public void APPROVER_CHANGE()
        {
            MoveWorkflowToAnyLevelDA moveWorkflowToAnyLevelDA = new MoveWorkflowToAnyLevelDA();
            moveWorkflowToAnyLevelDA.user = user;
            moveWorkflowToAnyLevelDA.APPROVER_CHANGE();

        }
        void APP_HISTORY_FOR_HR()
        {
            var from = Request["FromDate"].ToArabicDate();
            var to = Request["ToDate"].ToArabicDate();
            var Services = Request.Form["Service"];
            DBAccess ObjDb = new DBAccess();

            string strquery = @"

SELECT 
     ESS_LEAVE_REQ_DETAILS_ALL.*,to_char(ELRH_REQUESTED_DT,'dd/mm/yyyy')REQ_DATE,
      
          APPROVED_GROUP_NAME 
          CURR_STATUS_NAME_1,to_char(TO_DT,'dd/mm/yyyy') PROB_CYCLE_ENDDT, CONCAT(CONCAT(APP_DATE,' '),APP_TIME)APP_DATE,APP_TIME
    
   
FROM 
     ESS_LEAVE_REQ_DETAILS_ALL,
        ESS_REQ_APPROVED_GROUP,
      ( select AWDD_WF_HEADER_ROW_ID WF_HEADER_ID, SUM(CASE WHEN wd2.AWDD_STATUS in ('A','R') THEN 1 ELSE 0 END) AS APP_CNT
  from AUTH_WF_DOC_DETAIL wd2  
                    GROUP BY AWDD_WF_HEADER_ROW_ID)
    where
             UNIQUE_ID_1 =  TO_CHAR(LEAVE_REQ_ID)
                    AND UNIQUE_ID_2 = TO_CHAR(LEAVE_REQ_ID) 
    and WF_HEADER_ID(+) = AWH_ROW_ID1
    AND EST_SERVICE_ID in(20,21,22)           

and exists ( 
    SELECT 1 FROM AUTH_WF_HEADER, AUTH_WF_DOC_DETAIL
              WHERE     
                    AWDD_WF_HEADER_ROW_ID = AWH_ROW_ID
                    
                    AND AWH_UNIQUE_ID1 =  TO_CHAR(LEAVE_REQ_ID)
                    AND AWH_UNIQUE_ID2 = TO_CHAR(LEAVE_REQ_ID)
    )

             and((FROM_DT BETWEEN to_date('" + from.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy') AND to_date('" + to.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy') OR
            TO_DT BETWEEN to_date('" + from.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy') AND to_date('" + to.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy')) OR
                (FROM_DT < to_date('" + from.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy') AND to_date('" + to.ToString("dd/MM/yyyy") + @"', 'dd/mm/yyyy') < TO_DT))
    ORDER BY LEAVE_REQ_ID DESC";

            LogError.WriteToLogFile("ProbationCycle", "false", strquery);

            DataTable dt1 = objDB.execute_query_retun_datatable(strquery);
            if (dt1 != null && dt1.Rows.Count > 0)
            {
                var fname = "";
                 
                var JSONresult = JsonConvert.SerializeObject(dt1);
                Response.Write(JSONresult);
            }
            else
            {
                Response.Write("{}");
            }
        }

        private void WriteErrorMessage(string message)
        {
            Response.Write(APIResult<string>.ErrorResponse(message).GetResponse());
        }
        private void ResponseWrite(string message)
        {
            WriteErrorMessage(message);
        }
        private void WriteSuccessMessage(string message)
        {
            Response.Write(APIResult<string>.SuccessResponse(message).GetResponse());
        }

        public void MOVE_WORKFLOW_TO_THIS_LEVEL()
        {
            MoveWorkflowToAnyLevelDA moveWorkflowToAnyLevelDA = new MoveWorkflowToAnyLevelDA();
            moveWorkflowToAnyLevelDA.MOVE_WORKFLOW_TO_THIS_LEVEL();

        }


    }

}