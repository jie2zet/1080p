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
                case RecipeTemplateRegistry.InsituConditionRecipeFolder:
                    ModelTemplate = RecipeModelTemplate.InsituConditionRecipeTemplate;
                    break;
                case RecipeTemplateRegistry.ExsituConditionRecipeFolder:
                    ModelTemplate = RecipeModelTemplate.ExsituConditionRecipeTemplate;
                    break;
                case RecipeTemplateRegistry.InsituConditionSweepRecipeFolder:
                    ModelTemplate = RecipeModelTemplate.InsituContidionSweepRecipeTemplate;
                    break;
                case RecipeTemplateRegistry.ExsituConditionSweepRecipeFolder:
                    ModelTemplate = RecipeModelTemplate.ExsituContidionSweepRecipeTemplate;
                    break;
                case RecipeTemplateRegistry.DechuckRecipeFolder:
                    ModelTemplate = RecipeModelTemplate.DechuckRecipeTemplate;
                    break;
                case RecipeTemplateRegistry.DryRecipeFolder:
                    ModelTemplate = RecipeModelTemplate.DryRecipeTemplate;
                    break;
                case RecipeTemplateRegistry.HeadSweepRecipeFolder:
                    ModelTemplate = RecipeModelTemplate.HeadSweepRecipeTemplate;
                    break;
                case RecipeTemplateRegistry.LoadRecipeFolder:
                    ModelTemplate = RecipeModelTemplate.LoadRecipeTemplate;
                    break;
                case RecipeTemplateRegistry.PenRecipeFolder:
                    ModelTemplate = RecipeModelTemplate.PenA1RecipeTemplate;
                    break;
                case RecipeTemplateRegistry.PolishRecipeFolder:
                    ModelTemplate = RecipeModelTemplate.PolishRecipeTemplate;
                    break;
                case RecipeTemplateRegistry.RollRecipeFolder:
                    ModelTemplate = RecipeModelTemplate.RollRecipeTemplate;
                    break;
                case RecipeTemplateRegistry.UnloadRecipeFolder:
                    ModelTemplate = RecipeModelTemplate.UnloadRecipeTemplate;
                    break;
                case RecipeTemplateRegistry.LoadRetryRecipeFolder:
                    ModelTemplate = RecipeModelTemplate.LoadRetryRecipeTemplate;
                    break;
                case RecipeTemplateRegistry.DechuckRetryRecipeFolder:
                    ModelTemplate = RecipeModelTemplate.DechuckRetryRecipeTemplate;
                    break;
                case RecipeTemplateRegistry.UnloadRetryRecipeFolder:
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
