using CMP.Models.Recipe.RecipeEdit;
using DocumentFormat.OpenXml.Wordprocessing;
using HandyControl.Tools.Extension;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMP.ViewModels.Tabs.RecipeTab
{
    class AddRecipeEditorViewModel : BindableBase, IDialogResultable<Tuple<RecipeModelTemplate, string>>
    {
        private Tuple<RecipeModelTemplate, string> result;
        public Action? CloseAction { get; set; }
        public Tuple<RecipeModelTemplate, string> Result { get => result; set => SetProperty(ref result, value); }

        private string message;

        public string Message
        {
            get { return message; }
            set { message = value; GetDefualtTemplate(); RaisePropertyChanged(); }
        }

        private RecipeModelTemplate modelTemplate;

        public RecipeModelTemplate ModelTemplate
        {
            get { return modelTemplate; }
            set { modelTemplate = value; RaisePropertyChanged(); }
        }

        private int comboBoxIndex;

        public int ComboBoxIndex
        {
            get { return comboBoxIndex; }
            set { comboBoxIndex = value; RaisePropertyChanged(); }
        }


        private string name;

        public string Name
        {
            get { return name; }
            set { name = value; RaisePropertyChanged(); }
        }


        public DelegateCommand SelecteChangedCommand { get; set; }
        public DelegateCommand OkCmd => new(() =>
        {
            Result = new Tuple<RecipeModelTemplate, string>(ModelTemplate, Name);
            CloseAction?.Invoke();
        });
        public DelegateCommand CancelCmd => new(() =>
        {
            Result = new Tuple<RecipeModelTemplate, string>(RecipeModelTemplate.None, string.Empty);
            CloseAction?.Invoke();
        });

        public AddRecipeEditorViewModel()
        {
            result = new Tuple<RecipeModelTemplate, string>(RecipeModelTemplate.None, "");
            comboBoxIndex = 0;
            modelTemplate = RecipeModelTemplate.None;
            name = string.Empty;
            message = string.Empty;
            SelecteChangedCommand = new DelegateCommand(OnSelecteChanged);
        }

        private void GetDefualtTemplate()
        {
            switch (Message)
            {
                case "Condition Recipe":
                    ModelTemplate = RecipeModelTemplate.ConditionRecipeTemplate;
                    break;
                case "Condition Sweep Recipe":
                    ModelTemplate = RecipeModelTemplate.ExsituContidionSweepRecipeTemplate;
                    break;
                case "Dechuck Recipe":
                    ModelTemplate = RecipeModelTemplate.DechuckRecipeTemplate;
                    break;
                case "Dry Recipe":
                    ModelTemplate = RecipeModelTemplate.DryRecipeTemplate;
                    break;
                case "Head Sweep Recipe":
                    ModelTemplate = RecipeModelTemplate.HeadSweepRecipeTemplate;
                    break;
                case "Load Recipe":
                    ModelTemplate = RecipeModelTemplate.LoadRecipeTemplate;
                    break;
                case "Pen Recipe":
                    ModelTemplate = RecipeModelTemplate.PenA1RecipeTemplate;
                    break;
                case "Polish Recipe":
                    ModelTemplate = RecipeModelTemplate.PolishRecipeTemplate;
                    break;
                case "Roll Recipe":
                    ModelTemplate = RecipeModelTemplate.RollRecipeTemplate;
                    break;
                case "UnLoad Recipe":
                    ModelTemplate = RecipeModelTemplate.UnloadRecipeTemplate;
                    break;
                case "Load Retry Recipe":
                    ModelTemplate = RecipeModelTemplate.LoadRetryRecipeTemplate;
                    break;
                case "Dechuck Retry Recipe":
                    ModelTemplate = RecipeModelTemplate.DechuckRetryRecipeTemplate;
                    break;
                case "UnLoad Retry Recipe":
                    ModelTemplate = RecipeModelTemplate.UnloadRetryRecipeTemplate;
                    break;
                default:
                    ModelTemplate = RecipeModelTemplate.None;
                    break;
            }
            ComboBoxIndex = (int)ModelTemplate;
        }

        private void OnSelecteChanged()
        {

        }
    }
}
