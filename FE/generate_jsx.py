import re
import sys

def main():
    try:
        with open('src/components/MimoDashboard.jsx', 'r', encoding='utf-8') as f:
            content = f.read()

        # Insert state default
        content = re.sub(
            r"const \[activeTab, setActiveTab\] = useState\('Thư viện'\);",
            r"const [activeTab, setActiveTab] = useState('Bảng xếp hạng');",
            content
        )

        # Replace full-main condition
        content = re.sub(
            r"className=\{`mimo-layout \$\{activeTab === 'Nâng cấp' \? 'full-main' : ''\}`\}",
            r"className={`mimo-layout ${(activeTab === 'Nâng cấp' || activeTab === 'Hồ sơ') ? 'full-main' : ''}`}",
            content
        )

        # Replace right panel condition
        content = re.sub(
            r"\{activeTab !== 'Nâng cấp' && \(",
            r"{activeTab !== 'Nâng cấp' && activeTab !== 'Hồ sơ' && (",
            content
        )

        # Insert render calls in main
        content = re.sub(
            r"(\{activeTab === 'Thư viện' && renderLibraryContent\(\)\})",
            r"\1\n        {activeTab === 'Bảng xếp hạng' && renderLeaderboardContent()}\n        {activeTab === 'Hồ sơ' && renderProfileContent()}\n        {activeTab === 'Thành tích' && renderAchievementsContent()}",
            content
        )

        # Insert right panel calls
        content = re.sub(
            r"(\{activeTab === 'Thư viện' && renderLibraryRightPanel\(\)\})",
            r"\1\n          {activeTab === 'Bảng xếp hạng' && renderLeaderboardRightPanel()}\n          {activeTab === 'Thành tích' && renderAchievementsRightPanel()}",
            content
        )
        
        # Load the new functions from a separate file we'll write
        with open('new_render_functions.jsx', 'r', encoding='utf-8') as f:
            new_functions = f.read()
            
        # Insert them before the return statement
        content = content.replace("  return (", new_functions + "\n  return (")

        with open('src/components/MimoDashboard.jsx', 'w', encoding='utf-8') as f:
            f.write(content)
            
        print("Success")
    except Exception as e:
        print(f"Error: {e}")

if __name__ == '__main__':
    main()
