import matplotlib.pyplot as plt
import seaborn as sns
import pandas as pd


def get_data_from_file(fpath):
    data = []
    with open(fpath, "r") as file:
        for line in file:
            size, times = line.strip().split(':')
            size = int(size)
            times = times.split(',')
            times = [int(time) for time in times if time]
            for time in times:
                data.append({'Random Walk Size': size, 'Time (ms)': time})
    return pd.DataFrame(data)

fpath = 'results.txt'
df = get_data_from_file(fpath)

plt.figure(figsize=(15, 8))


#Switch these:
plt.xlim([df['Random Walk Size'].min() - 1, df['Random Walk Size'].max() + 1])
plt.ylim([0, df['Time (ms)'].max() * 1.1])


palette = sns.color_palette("coolwarm", len(df['Random Walk Size'].unique()))
boxplot = sns.boxplot(x='Random Walk Size', y='Time (ms)', data=df, palette=palette)

plt.xticks(rotation=45)
plt.title('Boxplot of Times for Different Random Walk Step Sizes', fontsize=16)
plt.xlabel('Random Walk Step Size', fontsize=14)


plt.ylabel('Time (ms)', fontsize=14)
plt.grid(True, linestyle='--')

plt.show()





