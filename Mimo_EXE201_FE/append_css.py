with open('src/components/MimoDashboard.css', 'a', encoding='utf-8') as f1:
    with open('new_styles.css', 'r', encoding='utf-8') as f2:
        f1.write('\n\n' + f2.read())
print("CSS appended successfully.")
