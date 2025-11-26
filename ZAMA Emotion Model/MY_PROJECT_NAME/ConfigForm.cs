using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ZAMAEmotionModel {

  public partial class ConfigForm : Form {

    private Configuration _cf;

    public ConfigForm() {

      InitializeComponent();
    }

    /// <summary>
    /// Loads configuration values into the form's UI controls.
    /// </summary>
    /// <param name="config">Configuration object to load from</param>
    public void SetConfiguration(Configuration config)
    {
      try
      {
        _cf = config;
        // Note: ConfigForm currently has no configurable fields
        // This is a placeholder for future configuration options
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message, "Error loading configuration");
      }
    }

    /// <summary>
    /// Returns the current configuration object.
    /// </summary>
    /// <returns>Configuration object with current settings</returns>
    public Configuration GetConfiguration()
    {
      return _cf;
    }

    /// <summary>
    /// Handles the Save button click. Saves configuration and closes dialog.
    /// </summary>
    private void btnSave_Click(object sender, EventArgs e)
    {
      try
      {
        // Note: ConfigForm currently has no configurable fields
        // This is a placeholder for future configuration options
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message, "Error saving configuration");
        return;
      }

      DialogResult = DialogResult.OK;
    }

    private void btnCancel_Click(object sender, EventArgs e) {

      DialogResult = DialogResult.Cancel;
    }
  }
}
