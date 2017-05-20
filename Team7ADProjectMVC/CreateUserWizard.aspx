<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="CreateUserWizard.aspx.cs" Inherits="Team7ADProjectMVC.CreateUserWizard" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body style="margin-right: 12px">
    <form id="form1" runat="server">
    <div>
    
    </div>
        <asp:CreateUserWizard ID="CreateUserWizard1" runat="server" OnCreatedUser="CreateUserWizard1_CreatedUser">
            <WizardSteps>
                <asp:CreateUserWizardStep runat="server" />
                <asp:CompleteWizardStep runat="server" />
            </WizardSteps>
        </asp:CreateUserWizard>
    </form>
</body>
</html>
