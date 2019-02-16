using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using Camelot.SharePointConnector.Data;
using System.Text.RegularExpressions;

namespace SurveyWebSiteExample
{
    public partial class SurveyControl : System.Web.UI.UserControl
    {
        public string ListName { get; set; }
        public string ConnectionString { get; set; }
        private DataTable _questions;

        protected void Page_Init(object sender, EventArgs e)
        {
            _questions = Helper.ExecuteDataTable("SHOW FIELDS FROM `" + ListName + "`.`" + ListName + "`", ConfigurationManager.ConnectionStrings[this.ConnectionString].ConnectionString);

            foreach (DataRow q in _questions.AsEnumerable().Where(r => (bool)r["VisibleInView"] == true && (bool)r["ReadOnly"] == false && (string)r["Name"] != "ContentType"))
            {
                var step = new WizardStep();

                if ((bool)q["Required"])
                    step.Controls.Add(new LiteralControl("<h2>" + q["DisplayName"] + "<font color=\"red\"> *</font></h2>"));
                else
                    step.Controls.Add(new LiteralControl("<h2>" + q["DisplayName"] + "</h2>"));

                SurveyWizard.WizardSteps.Insert(SurveyWizard.WizardSteps.Count - 1, step);

                switch ((string)q["Type"])
                {
                    case "Choice":
                        var singleChoice = new RadioButtonList();
                        singleChoice.ID = ((Guid)q["ID"]).ToString();

                        foreach (Match choice in Regex.Matches((string)q["EnumValues"], @"\[(.*?)\]"))
                            singleChoice.Items.Add(choice.Groups[1].Value);

                        step.Controls.Add(singleChoice);
                        break;
                    case "MultiChoice":
                        var multiChoice = new CheckBoxList();
                        multiChoice.ID = ((Guid)q["ID"]).ToString();

                        foreach (Match choice in Regex.Matches((string)q["EnumValues"], @"\[(.*?)\]"))
                            multiChoice.Items.Add(choice.Groups[1].Value);

                        step.Controls.Add(multiChoice);
                        break;
                    case "Text":
                        var singleLineText = new TextBox();
                        singleLineText.ID = ((Guid)q["ID"]).ToString();

                        step.Controls.Add(singleLineText);
                        break;
                    case "Boolean":
                        var yesNo = new RadioButtonList();
                        yesNo.ID = ((Guid)q["ID"]).ToString();

                        yesNo.Items.Add("Yes");
                        yesNo.Items.Add("No");

                        step.Controls.Add(yesNo);
                        break;
                    default:
                        break;
                }
            }
        }

        protected void NextButtonClicked(object sender, WizardNavigationEventArgs e)
        {
            if (SurveyWizard.ActiveStepIndex == 0) return;
                 
            var index = 0;
            foreach (DataRow q in _questions.AsEnumerable().Where(r => (bool)r["VisibleInView"] == true && (bool)r["ReadOnly"] == false && (string)r["Name"] != "ContentType"))
            {
                if (index == SurveyWizard.ActiveStepIndex - 1)
                {
                    var control = SurveyWizard.ActiveStep.FindControl(((Guid)q["ID"]).ToString());

                    if (control != null)
                    {
                        string value = null;

                        switch ((string)q["Type"])
                        {
                            case "Choice":
                                value = ((RadioButtonList)control).SelectedValue;
                                break;
                            case "MultiChoice":
                                value = ((CheckBoxList)control).Items.Cast<ListItem>().Where(m => m.Selected == true).Select(m => m.Value).FirstOrDefault();
                                break;
                            case "Text":
                                value = ((TextBox)control).Text;
                                break;
                            case "Boolean":
                                value = ((RadioButtonList)control).SelectedValue == "Yes" ? "true" : "false";
                                break;
                            default:
                                break;
                        }

                        if ((bool)q["Required"] && string.IsNullOrEmpty(value))
                        {
                            e.Cancel = true;
                            return;
                        }
                    }

                    return;
                }
                index++;
            }
        }

        protected void FinishButtonClicked(object sender, WizardNavigationEventArgs e)
        {
            NextButtonClicked(sender, e);
            if (e.Cancel) return;

            var answers = new Dictionary<string, object>();

            foreach (DataRow q in _questions.AsEnumerable().Where(r => (bool)r["VisibleInView"] == true && (bool)r["ReadOnly"] == false && (string)r["Name"] != "ContentType"))
            {
                var control = SurveyWizard.FindControl(((Guid)q["ID"]).ToString());

                switch ((string)q["Type"])
                {
                    case "Choice":
                        string singleChoiceAnswer = ((RadioButtonList)control).SelectedValue;
                        answers.Add(((Guid)q["ID"]).ToString("N"), singleChoiceAnswer);
                        break;
                    case "MultiChoice":
                        string[] multiChoiceAnswer = ((CheckBoxList)control).Items.Cast<ListItem>().Where(m => m.Selected == true).Select(m => m.Value).ToArray();
                        answers.Add(((Guid)q["ID"]).ToString("N"), multiChoiceAnswer);
                        break;
                    case "Text":
                        string singleLineTextAnswer = ((TextBox)control).Text;
                        answers.Add(((Guid)q["ID"]).ToString("N"), singleLineTextAnswer);
                        break;
                    case "Boolean":
                        bool yesNoAnswer = ((RadioButtonList)control).SelectedValue == "Yes" ? true : false;
                        answers.Add(((Guid)q["ID"]).ToString("N"), yesNoAnswer);
                        break;
                    default:
                        break;
                }
            }

            var insertCommand = "INSERT INTO `" + ListName + "` SET " + _questions.AsEnumerable().Where(r => (bool)r["VisibleInView"] == true && (bool)r["ReadOnly"] == false && (string)r["Name"] != "ContentType").Select(m => "`" + (string)m["Name"] + "`" + " = @" + ((Guid)m["ID"]).ToString("N")).Aggregate((a, b) => a + ", " + b);
            using (var connection = new SharePointConnection(ConfigurationManager.ConnectionStrings[this.ConnectionString].ConnectionString))
            {
                connection.Open();

                using (var command = new SharePointCommand(insertCommand, connection))
                {
                    foreach (var answer in answers)
                        command.Parameters.Add("@" + answer.Key, answer.Value);

                    command.ExecuteNonQuery();
                }
            }
        }
    }
}