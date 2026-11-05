# psvclitescript.py

# ✅ This SikuliX script runs inside VDI to open PS VC Lite application
 
import time
 
print("🚀 Sikuli script started to open PS VC Lite")
 
# ---------------------------------------------

# 🖥️ Step 1: Wait for VDI Desktop to appear

# ---------------------------------------------
 
 
# ---------------------------------------------

# 💻 Step 2: Wait for PS VC Lite icon to appear

# ---------------------------------------------

try:

    # wait() again waits only as long as needed (max 60s)

    psvc_icon = wait("PS_VC_Lite_Icon.png", 360)

    click(psvc_icon)

    print("✅ PS VC Lite icon found and clicked.")

except FindFailed:

    print("❌ PS VC Lite icon not found within 1 minute.")

    exit(1)
 
# ---------------------------------------------

# 🎯 Step 3: Final confirmation

# ---------------------------------------------

print("🎉 PS VC Lite should now be launching...")

time.sleep(3)

print("✅ Script completed successfully.")
 
# Step 1️⃣ – Double-click on target field to focus
type("a", Key.CTRL)
wait(1)
if exists("Script.png", 10):
 
    doubleClick("Script.png")
 
    wait(1)
    # Step 3️⃣ – Select All + Delete
 
    type("a", Key.CTRL)
 
    wait(0.5)
 
    type("c", Key.CTRL)
 
    wait(0.5)
 
# Step 1️⃣ – Find and double-click the target image
 
if exists("File.png", 10):
 
    click("File.png")
 
    wait(1)
 
# Step 2️⃣ – Click inside editable area
 
if exists("Exit.png", 10):
 
    click("Exit.png")
 
    wait(10)
type("a", Key.CTRL)
wait(1)
if exists("APP.png", 10):
 
    doubleClick("APP.png")
 
    wait(30)
 
if exists("selectproject.png", 10):
 
        click("selectproject.png")
 
        wait(30)
 
if exists("Viewers.png", 10):
 
        doubleClick("Viewers.png")
 
        wait(2)
 
if exists("ProgramScript.png", 10):
 
        click("ProgramScript.png")
 
        wait(2)
 
if exists("selectprogram.png", 10):
 
        click("selectprogram.png")
 
        wait(2)
 
# Step 3️⃣ – Select All
 
        type("a", Key.CTRL)
 
        wait(2)
 
        type(Key.DELETE)
 
        wait(2)
        # Paste ExcelDataList content
 
        type("v", Key.CTRL)
 
        wait(2)
 
        type("s", Key.CTRL)
 
        wait(2)
 
if exists("execute.png", 10):
 
        click("execute.png")
 
        wait(2)
 
if exists("Executecode.png", 10):
 
        click("Executecode.png")
 
        wait(2)
