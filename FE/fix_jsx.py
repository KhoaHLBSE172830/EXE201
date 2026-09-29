import re

with open('src/components/MimoDashboard.jsx', 'r', encoding='utf-8') as f:
    content = f.read()

# The first injection starts after `document.addEventListener('mousedown', handleClickOutside);`
# and we know the original was `    return () => document.removeEventListener('mousedown', handleClickOutside);\n  }, []);`

# Let's see if we can find it
idx1 = content.find("document.addEventListener('mousedown', handleClickOutside);")
idx2 = content.find("  const renderHomeContent = () => (")

if idx1 != -1 and idx2 != -1:
    # replace everything between idx1 + len(...) and idx2
    start = idx1 + len("document.addEventListener('mousedown', handleClickOutside);\n")
    end = idx2
    
    # Check if there is really an injected block here
    if "const renderLeaderboardContent" in content[start:end]:
        new_content = content[:start] + "    return () => document.removeEventListener('mousedown', handleClickOutside);\n  }, []);\n\n" + content[end:]
        with open('src/components/MimoDashboard.jsx', 'w', encoding='utf-8') as f:
            f.write(new_content)
        print("Fixed MimoDashboard.jsx")
    else:
        print("No injected block found at the expected location.")
else:
    print("Could not find the anchors.")

