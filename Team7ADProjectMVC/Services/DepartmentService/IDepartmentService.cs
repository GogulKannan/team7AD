using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Team7ADProjectMVC.Services
{
    //Author: Sandi
    interface IDepartmentService
    {
        List<Employee> GetStoreManagerAndSupervisor();
        void UpdateEmployee(Employee e);
        List<Employee> GetAllEmployees();
        List<Department> ListAllDepartments();
        Department FindDeptById(int id);
        Employee FindEmployeeById(int id);
        void changeDeptCp(Department department, int cpId);
        List<RequisitionDetail> GetRequisitionDetailByDept(int dId, int rId);

        //Change Rep Methods
        Employee GetCurrentRep(int? depIdofLoginUser);
        List<Employee> GetAllEmployee(int? depIdofLoginUser, int currentRepId);
        Employee GetEmpbyId(int? empIdforRep);
        void ChangeRep(Employee currentRep, Employee newRep);

        //Delegate Methods
        List<Delegate> getDelegate();
        List<Employee> GetAllEmployeebyDepId(int? depId);
        Employee FindById(int? empid);
        void TerminateDelegate(Delegate del);
        Delegate FinddelegaterecordById(int? delegateId);
        Delegate getDelegatedEmployee(int? depId);
        void manageDelegate(Employee e, DateTime startDate, DateTime endDate, int? depHeadId);
        void updateDelegate(Delegate d, DateTime startDate, DateTime endDate, int? depHeadId);
        bool IsDelegate(Employee e);
        Employee SetDelegatePermissions(Employee e);
        List<Employee> GetEverySingleEmployeeInDepartment(int? depId);
        List<CollectionPoint> getAllCollectionPoint();
    }
}
