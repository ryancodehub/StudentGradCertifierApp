using System.Windows;

namespace StudentGradCertifier.FunctionWindows
{
    /// <summary>
    /// Idea is to pull a list of students from PowerCampus who are in Applied status
    /// Then present that list to the users (maybe write it out to a csv or something for them?)
    /// Then let them run the certifier against that list
    /// </summary>
    public partial class CertifyPowerCampus : Window
    {
        public CertifyPowerCampus()
        {
            InitializeComponent();
        }
    }
}
