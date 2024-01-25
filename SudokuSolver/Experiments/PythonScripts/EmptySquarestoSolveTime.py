import pandas as pd
import matplotlib.pyplot as plt

data = {
    3: [29047,5110,9245,11515,22853,1349,3168,45880,5219,5493,7231,9574,1431,2298,10571,6612,26290,7507,12356,14054,8443,7028,32522,1048,13492],
    4: [685,1313,1824,756,811,3473,5148,6096,1015,484,7842,1666,11695,1421,702,3013,1733,227,3956,172,622,709,5054,5796,644],
    2: [448,5790,2014,905,1377,393,2591,168,3915,391,6234,5170,1688,3075,1960,1015,3621,4139,67,739,3402,1190,947,550,3773],
    1: [405,405,580,197,856,534,942,191,268,497,1169,739,88,147,301,1401,67,562,392,227,176,245,1688,1811,3314],
    5: [296,44,43,214,26,54,128,99,45,62,78,145,95,68,43,17,60,84,149,91,34,14,75,60,35]
}

df = pd.DataFrame(data).melt(var_name='Puzzle Number', value_name='Time (ms)')

filled_squares = {1: 32, 2: 30, 3: 28, 4: 30, 5: 36}
average_times = df.groupby('Puzzle Number')['Time (ms)'].mean()

avg_times_vs_filled = {filled_squares[key]: val for key, val in average_times.items()}
sorted_filled = sorted(avg_times_vs_filled.keys())
sorted_times = [avg_times_vs_filled[square] for square in sorted_filled]
plt.figure(figsize=(10,6))
plt.plot(sorted_filled,sorted_times,marker='o', linestyle='-', color='red')



plt.title('Relation between prefilled squares and mean solve time', fontsize=16)
plt.xlabel('Prefilled Squares', fontsize=14)
plt.ylabel('Time (ms)', fontsize=14)
plt.grid(True, linestyle='--', alpha=0.7)
plt.tight_layout()
plt.show()
