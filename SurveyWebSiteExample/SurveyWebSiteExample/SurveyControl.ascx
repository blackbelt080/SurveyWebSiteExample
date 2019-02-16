<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="SurveyControl.ascx.cs" Inherits="SurveyWebSiteExample.SurveyControl" %>

<style type="text/css">
    .survey
    {
        font-family:trebuchet ms;        
        padding:10px;           
    }
</style>

<div style="margin-left:auto; margin-right:auto; margin-top:100px; width:700px; overflow:hidden; border:solid black 1px;">
    <div><img src="images/cat.jpg" width="100%" alt="" /></div>
    <div class="survey">
        <asp:Wizard ID="SurveyWizard" runat="server" DisplaySideBar="false" OnFinishButtonClick="FinishButtonClicked" OnNextButtonClick="NextButtonClicked">
        <WizardSteps>
            <asp:WizardStep ID="StartStep" runat="server">
                <p>We would like to ask you a few questions about cats. This survey will help you find what temperament of cat is a good match for you. Please click next to continue!</p>
            </asp:WizardStep>
            <asp:WizardStep ID="EndStep" runat="server" StepType="Complete" AllowReturn="false" >
                <p>Thank you for participating!</p>
            </asp:WizardStep>
        </WizardSteps>
        </asp:Wizard>
    </div>
</div>