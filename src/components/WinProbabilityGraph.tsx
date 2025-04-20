import React from 'react';
import { Line } from 'react-chartjs-2';
import {
    Chart as ChartJS,
    CategoryScale,
    LinearScale,
    PointElement,
    LineElement,
    Title,
    Tooltip,
    Legend,
    ChartOptions,
    ScaleType,
    Scale
} from 'chart.js';

// Register Chart.js components
ChartJS.register(
    CategoryScale,
    LinearScale,
    PointElement,
    LineElement,
    Title,
    Tooltip,
    Legend
);

interface WinProbabilityGraphProps {
    probabilities: number[];
    currentMove: number;
}

const WinProbabilityGraph: React.FC<WinProbabilityGraphProps> = ({ probabilities, currentMove }) => {
    const data = {
        labels: Array.from({ length: probabilities.length }, (_, i) => `Move ${i + 1}`),
        datasets: [
            {
                label: 'Win Probability',
                data: probabilities,
                borderColor: '#00ff00',
                backgroundColor: 'rgba(0, 255, 0, 0.1)',
                fill: true,
                tension: 0.4,
                pointRadius: (ctx: any) => {
                    // Make the current move point larger
                    return ctx.dataIndex === currentMove ? 6 : 3;
                },
                pointBackgroundColor: (ctx: any) => {
                    // Highlight current move point
                    return ctx.dataIndex === currentMove ? '#fff' : '#00ff00';
                },
                pointBorderColor: '#00ff00',
                pointBorderWidth: (ctx: any) => {
                    return ctx.dataIndex === currentMove ? 2 : 1;
                },
            }
        ]
    };

    const options: ChartOptions<'line'> = {
        responsive: true,
        maintainAspectRatio: false,
        scales: {
            y: {
                beginAtZero: true,
                max: 100,
                ticks: {
                    callback: function(value) {
                        return `${value}%`;
                    },
                    color: '#fff',
                },
                grid: {
                    color: 'rgba(255, 255, 255, 0.1)',
                }
            },
            x: {
                ticks: {
                    color: '#fff',
                },
                grid: {
                    color: 'rgba(255, 255, 255, 0.1)',
                }
            }
        },
        plugins: {
            legend: {
                display: false,
            },
            tooltip: {
                callbacks: {
                    label: function(context) {
                        return `Win Probability: ${context.parsed.y.toFixed(1)}%`;
                    }
                }
            }
        }
    };

    return (
        <div style={{
            backgroundColor: 'rgba(0, 0, 0, 0.8)',
            borderRadius: '15px',
            padding: '20px',
            marginTop: '20px',
            height: '200px'
        }}>
            <h3 style={{
                color: '#00ff00',
                textAlign: 'center',
                marginBottom: '10px',
                fontFamily: 'monospace'
            }}>
                Win Probability
            </h3>
            <Line data={data} options={options} />
        </div>
    );
};

export default WinProbabilityGraph; 