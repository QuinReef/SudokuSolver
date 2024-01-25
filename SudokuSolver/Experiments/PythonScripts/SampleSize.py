import matplotlib.pyplot as plt

def get_data_from_sampleTest(inputStr):
    lines = inputStr.strip().split('\n')
    data = {}

    current_sample_size = None;
    for line in lines:
        if line.endswith(":"):
            current_sample_size = int(line[:-1])
            data[current_sample_size] = {}
        else:
            pair = line
            if pair:
                step_size, times = pair.split(':')
                step_size = int(step_size.strip())
                realtimes = []
                for t in times:
                    if t and t != ',':
                        realtimes.append(int(t.strip()))
                data[current_sample_size][step_size] = realtimes
    return data

def plot_data(data):
    plt.figure(figsize=(12,8))
    for sample_size, step_data in data.items():
        avg_times = [sum(times) / len(times) for step, times in step_data.items()]
        step_sizes = list(step_data.keys())

        plt.plot(step_sizes,avg_times,label=f'Sample Size {sample_size}')
    plt.xlabel('Step Size')
    plt.ylabel('Avg Time (ms)')
    plt.title('Effect of sample size')
    plt.legend()
    plt.grid(True)
    plt.show()

input_str = None;
with open('means.txt') as f:
    input_str = f.read()
plot_data(get_data_from_sampleTest(input_str))