using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace mRemoteNG.UI.TaskDialog
{
  public partial class frmTaskDialog : Form
  {
    //--------------------------------------------------------------------------------
    #region PRIVATE members
    //--------------------------------------------------------------------------------
    ESysIcons _mainIcon = ESysIcons.Question;
    ESysIcons _footerIcon = ESysIcons.Warning;

    string _mainInstruction = "Main Instruction Text";
    int _mainInstructionHeight = 0;
    Font _mainInstructionFont = new Font("Segoe UI", 11.75F, FontStyle.Regular, GraphicsUnit.Point, (byte)0);

    List<RadioButton> _radioButtonCtrls = new List<RadioButton>();
    string _radioButtons = "";
    int _initialRadioButtonIndex = 0;

    List<Button> _cmdButtons = new List<Button>();
    string _commandButtons = "";
    int _commandButtonClicked = -1;

    int _defaultButtonIndex = 0;
    Control _focusControl = null;

    ETaskDialogButtons _buttons = ETaskDialogButtons.YesNoCancel;

    bool _expanded = false;
    bool _isVista = false;
    #endregion

    //--------------------------------------------------------------------------------
    #region PROPERTIES
    //--------------------------------------------------------------------------------
    public ESysIcons MainIcon { get { return _mainIcon; } set { _mainIcon = value; } }
    public ESysIcons FooterIcon { get { return _footerIcon; } set { _footerIcon = value; } }

    public string Title { get { return Text; } set { Text = value; } }
    public string MainInstruction { get { return _mainInstruction; } set { _mainInstruction = value; Invalidate(); } }
    public string Content { get { return lbContent.Text; } set { lbContent.Text = value; } }
    public string ExpandedInfo { get { return lbExpandedInfo.Text; } set { lbExpandedInfo.Text = value; } }
    public string Footer { get { return lbFooter.Text; } set { lbFooter.Text = value; } }
    public int DefaultButtonIndex { get { return _defaultButtonIndex; } set { _defaultButtonIndex = value; } }

    public string RadioButtons { get { return _radioButtons; } set { _radioButtons = value; } }
    public int InitialRadioButtonIndex { get { return _initialRadioButtonIndex; } set { _initialRadioButtonIndex = value; } }
    public int RadioButtonIndex
    {
      get
      {
        foreach (RadioButton rb in _radioButtonCtrls)
          if (rb.Checked)
            return (int)rb.Tag;
        return -1;
      }
    }

    public string CommandButtons { get { return _commandButtons; } set { _commandButtons = value; } }
    public int CommandButtonClickedIndex => _commandButtonClicked;

      public ETaskDialogButtons Buttons { get { return _buttons; } set { _buttons = value; } }

    public string VerificationText { get { return cbVerify.Text; } set { cbVerify.Text = value; } }
    public bool VerificationCheckBoxChecked { get { return cbVerify.Checked; } set { cbVerify.Checked = value; } }

    public bool Expanded { get { return _expanded; } set { _expanded = value; } }
    #endregion

    //--------------------------------------------------------------------------------
    #region CONSTRUCTOR
    //--------------------------------------------------------------------------------
    public frmTaskDialog()
    {
      InitializeComponent();

     // _isVista = VistaTaskDialog.IsAvailableOnThisOS;
      if (!_isVista && CTaskDialog.UseToolWindowOnXp) // <- shall we use the smaller toolbar?
          FormBorderStyle = FormBorderStyle.FixedToolWindow;

      MainInstruction = "Main Instruction";
      Content = "";
      ExpandedInfo = "";
      Footer = "";
      VerificationText = "";
    }
    #endregion 

    //--------------------------------------------------------------------------------
    #region BuildForm
    // This is the main routine that should be called before .ShowDialog()
    //--------------------------------------------------------------------------------
    bool _formBuilt = false;
    public void BuildForm()
    {
      int form_height = 0;

      // Setup Main Instruction
      switch (_mainIcon)
      {
        case ESysIcons.Information: imgMain.Image = SystemIcons.Information.ToBitmap(); break;
        case ESysIcons.Question: imgMain.Image = SystemIcons.Question.ToBitmap(); break;
        case ESysIcons.Warning: imgMain.Image = SystemIcons.Warning.ToBitmap(); break;
        case ESysIcons.Error: imgMain.Image = SystemIcons.Error.ToBitmap(); break;
      }

      //AdjustLabelHeight(lbMainInstruction);
      //pnlMainInstruction.Height = Math.Max(41, lbMainInstruction.Height + 16);
      if (_mainInstructionHeight == 0)
        GetMainInstructionTextSizeF();
      pnlMainInstruction.Height = Math.Max(41, _mainInstructionHeight + 16);

      form_height += pnlMainInstruction.Height;

      // Setup Content
      pnlContent.Visible = (Content != "");
      if (Content != "")
      {
        AdjustLabelHeight(lbContent);
        pnlContent.Height = lbContent.Height + 4;
        form_height += pnlContent.Height;
      }

      bool show_verify_checkbox = (cbVerify.Text != "");
      cbVerify.Visible = show_verify_checkbox;

      // Setup Expanded Info and Buttons panels
      if (ExpandedInfo == "")
      {
        pnlExpandedInfo.Visible = false;
        lbShowHideDetails.Visible = false;
        cbVerify.Top = 12;
        pnlButtons.Height = 40;
      }
      else
      {
        AdjustLabelHeight(lbExpandedInfo);
        pnlExpandedInfo.Height = lbExpandedInfo.Height + 4;
        pnlExpandedInfo.Visible = _expanded;
        lbShowHideDetails.Text = (_expanded ? "        Hide details" : "        Show details");
        lbShowHideDetails.ImageIndex = (_expanded ? 0 : 3);
        if (!show_verify_checkbox)
          pnlButtons.Height = 40;
        if (_expanded)
          form_height += pnlExpandedInfo.Height;
      }

      // Setup RadioButtons
      pnlRadioButtons.Visible = (_radioButtons != "");
      if (_radioButtons != "")
      {
        string[] arr = _radioButtons.Split(new char[] { '|' });
        int pnl_height = 12;
        for (int i = 0; i < arr.Length; i++)
        {
          RadioButton rb = new RadioButton();
          rb.Parent = pnlRadioButtons;
          rb.Location = new Point(60, 4 + (i * rb.Height));
          rb.Text = arr[i];
          rb.Tag = i;
          rb.Checked = (_defaultButtonIndex == i);
          rb.Width = Width - rb.Left - 15;
          pnl_height += rb.Height;
          _radioButtonCtrls.Add(rb);
        }
        pnlRadioButtons.Height = pnl_height;
        form_height += pnlRadioButtons.Height;
      }

      // Setup CommandButtons
      pnlCommandButtons.Visible = (_commandButtons != "");
      if (_commandButtons != "")
      {
        string[] arr = _commandButtons.Split(new char[] { '|' });
        int t = 8;
        int pnl_height = 16;
        for (int i = 0; i < arr.Length; i++)
        {
          CommandButton btn = new CommandButton();
          btn.Parent = pnlCommandButtons;
          btn.Location = new Point(50, t);
          if (_isVista)  // <- tweak font if vista
            btn.Font = new Font(btn.Font, FontStyle.Regular);
          btn.Text = arr[i];
          btn.Size = new Size(Width - btn.Left - 15, btn.GetBestHeight());
          t += btn.Height;
          pnl_height += btn.Height;
          btn.Tag = i;
          btn.Click += new EventHandler(CommandButton_Click);
          if (i == _defaultButtonIndex)
            _focusControl = btn;
        }
        pnlCommandButtons.Height = pnl_height;
        form_height += pnlCommandButtons.Height;
      }

      // Setup Buttons
      switch (_buttons)
      {
        case ETaskDialogButtons.YesNo:
          bt1.Visible = false;
          bt2.Text = "&Yes";
          bt2.DialogResult = DialogResult.Yes;
          bt3.Text = "&No";
          bt3.DialogResult = DialogResult.No;
          AcceptButton = bt2;
          CancelButton = bt3;
          break;
        case ETaskDialogButtons.YesNoCancel:
          bt1.Text = "&Yes";
          bt1.DialogResult = DialogResult.Yes;
          bt2.Text = "&No";
          bt2.DialogResult = DialogResult.No;
          bt3.Text = "&Cancel";
          bt3.DialogResult = DialogResult.Cancel;
          AcceptButton = bt1;
          CancelButton = bt3;
          break;
        case ETaskDialogButtons.OkCancel:
          bt1.Visible = false;
          bt2.Text = "&OK";
          bt2.DialogResult = DialogResult.OK;
          bt3.Text = "&Cancel";
          bt3.DialogResult = DialogResult.Cancel;
          AcceptButton = bt2;
          CancelButton = bt3;
          break;
        case ETaskDialogButtons.Ok:
          bt1.Visible = false;
          bt2.Visible = false;
          bt3.Text = "&OK";
          bt3.DialogResult = DialogResult.OK;
          AcceptButton = bt3;
          CancelButton = bt3;
          break;
        case ETaskDialogButtons.Close:
          bt1.Visible = false;
          bt2.Visible = false;
          bt3.Text = "&Close";
          bt3.DialogResult = DialogResult.Cancel;
          CancelButton = bt3;
          break;
        case ETaskDialogButtons.Cancel:
          bt1.Visible = false;
          bt2.Visible = false;
          bt3.Text = "&Cancel";
          bt3.DialogResult = DialogResult.Cancel;
          CancelButton = bt3;
          break;
        case ETaskDialogButtons.None:
          bt1.Visible = false;
          bt2.Visible = false;
          bt3.Visible = false;
          break;
      }

      ControlBox = (Buttons == ETaskDialogButtons.Cancel ||
                         Buttons == ETaskDialogButtons.Close ||
                         Buttons == ETaskDialogButtons.OkCancel ||
                         Buttons == ETaskDialogButtons.YesNoCancel);

      if (!show_verify_checkbox && ExpandedInfo == "" && _buttons == ETaskDialogButtons.None)
        pnlButtons.Visible = false;
      else
        form_height += pnlButtons.Height;

      pnlFooter.Visible = (Footer != "");
      if (Footer != "")
      {
        AdjustLabelHeight(lbFooter);
        pnlFooter.Height = Math.Max(28, lbFooter.Height + 16);
        switch (_footerIcon)
        {
          case ESysIcons.Information:
            // SystemIcons.Information.ToBitmap().GetThumbnailImage(16, 16, null, IntPtr.Zero);
            imgFooter.Image = ResizeBitmap(SystemIcons.Information.ToBitmap(), 16, 16);
            break;
          case ESysIcons.Question:
            // SystemIcons.Question.ToBitmap().GetThumbnailImage(16, 16, null, IntPtr.Zero);
            imgFooter.Image = ResizeBitmap(SystemIcons.Question.ToBitmap(), 16, 16);
            break;
          case ESysIcons.Warning:
            // SystemIcons.Warning.ToBitmap().GetThumbnailImage(16, 16, null, IntPtr.Zero);
            imgFooter.Image = ResizeBitmap(SystemIcons.Warning.ToBitmap(), 16, 16);
            break;
          case ESysIcons.Error:
            // SystemIcons.Error.ToBitmap().GetThumbnailImage(16, 16, null, IntPtr.Zero);
            imgFooter.Image = ResizeBitmap(SystemIcons.Error.ToBitmap(), 16, 16);
            break;
        }
        form_height += pnlFooter.Height;
      }

      ClientSize = new Size(ClientSize.Width, form_height);

      _formBuilt = true;
    }

    //--------------------------------------------------------------------------------
    Image ResizeBitmap(Image SrcImg, int NewWidth, int NewHeight)
    {
      float percent_width = (NewWidth / (float)SrcImg.Width);
      float percent_height = (NewHeight / (float)SrcImg.Height);

      float resize_percent = (percent_height < percent_width ? percent_height : percent_width);

      int w = (int)(SrcImg.Width * resize_percent);
      int h = (int)(SrcImg.Height * resize_percent);
      Bitmap b = new Bitmap(w, h);

      using (Graphics g = Graphics.FromImage(b))
      {
        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
        g.DrawImage(SrcImg, 0, 0, w, h);
      }
      return b;
    }
    
    //--------------------------------------------------------------------------------
    // utility function for setting a Label's height
    void AdjustLabelHeight(Label lb)
    {
      string text = lb.Text;
      Font textFont = lb.Font;
      SizeF layoutSize = new SizeF(lb.ClientSize.Width, 5000.0F);
      Graphics g = Graphics.FromHwnd(lb.Handle);
      SizeF stringSize = g.MeasureString(text, textFont, layoutSize);
      lb.Height = (int)stringSize.Height + 4;
      g.Dispose();
    }
    #endregion

    //--------------------------------------------------------------------------------
    #region EVENTS
    //--------------------------------------------------------------------------------
    void CommandButton_Click(object sender, EventArgs e)
    {
     	_commandButtonClicked = (int)((CommandButton)sender).Tag;
      DialogResult = DialogResult.OK;
    }

    //--------------------------------------------------------------------------------
    protected override void OnLoad(EventArgs e)
    {
      base.OnLoad(e);
    }

    //--------------------------------------------------------------------------------
    protected override void OnShown(EventArgs e)
    {
      if (!_formBuilt)
        throw new Exception("frmTaskDialog : Please call .BuildForm() before showing the TaskDialog");
      base.OnShown(e);
    }

    //--------------------------------------------------------------------------------
    private void lbDetails_MouseEnter(object sender, EventArgs e)
    {
      lbShowHideDetails.ImageIndex = (_expanded ? 1 : 4);
    }

    //--------------------------------------------------------------------------------
    private void lbDetails_MouseLeave(object sender, EventArgs e)
    {
      lbShowHideDetails.ImageIndex = (_expanded ? 0 : 3);
    }

    //--------------------------------------------------------------------------------
    private void lbDetails_MouseUp(object sender, MouseEventArgs e)
    {
      lbShowHideDetails.ImageIndex = (_expanded ? 1 : 4);
    }

    //--------------------------------------------------------------------------------
    private void lbDetails_MouseDown(object sender, MouseEventArgs e)
    {
      lbShowHideDetails.ImageIndex =(_expanded ? 2 : 5);
    }

    //--------------------------------------------------------------------------------
    private void lbDetails_Click(object sender, EventArgs e)
    {
      _expanded = !_expanded;
      pnlExpandedInfo.Visible = _expanded;
      lbShowHideDetails.Text = (_expanded ? "        Hide details" : "        Show details");
      if (_expanded)
        Height += pnlExpandedInfo.Height;
      else
        Height -= pnlExpandedInfo.Height;
    }

    //--------------------------------------------------------------------------------
    const int MAIN_INSTRUCTION_LEFT_MARGIN = 46;
    const int MAIN_INSTRUCTION_RIGHT_MARGIN = 8;

    SizeF GetMainInstructionTextSizeF()
    {
      SizeF mzSize = new SizeF(pnlMainInstruction.Width - MAIN_INSTRUCTION_LEFT_MARGIN - MAIN_INSTRUCTION_RIGHT_MARGIN, 5000.0F);
      Graphics g = Graphics.FromHwnd(Handle);
      SizeF textSize = g.MeasureString(_mainInstruction, _mainInstructionFont, mzSize);
      _mainInstructionHeight = (int)textSize.Height;
      return textSize;
    }

    private void pnlMainInstruction_Paint(object sender, PaintEventArgs e)
    {
      SizeF szL = GetMainInstructionTextSizeF();
      e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
      e.Graphics.DrawString(_mainInstruction, _mainInstructionFont, new SolidBrush(Color.DarkBlue), new RectangleF(new PointF(MAIN_INSTRUCTION_LEFT_MARGIN, 10), szL));
    }

    //--------------------------------------------------------------------------------
    private void frmTaskDialog_Shown(object sender, EventArgs e)
    {
      if (CTaskDialog.PlaySystemSounds)
      {
        switch (_mainIcon)
        {
          case ESysIcons.Error: System.Media.SystemSounds.Hand.Play(); break;
          case ESysIcons.Information: System.Media.SystemSounds.Asterisk.Play(); break;
          case ESysIcons.Question: System.Media.SystemSounds.Asterisk.Play(); break;
          case ESysIcons.Warning: System.Media.SystemSounds.Exclamation.Play(); break;
        }
      }
      if (_focusControl != null)
        _focusControl.Focus();
    }

    #endregion

    //--------------------------------------------------------------------------------
  }
}
