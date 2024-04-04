using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SharpC8;
public  class Chip8 {
    //Setup for various variables needed for the CPU to cycle.
    public bool drawFlag = false;
    private short opcode = 0;
    private byte[] registers = new byte[16];
    private byte[] memory = new byte[2048];

    private short I = 0;
    private short pc = 0x200;

    //The timers are intended to count down at a speed of 60hz.
    public  int delayTimer = 0;
    public  int soundTimer = 0;

    //Graphics interfact - this will be used by the program to update the graphics via assigning 1 or 0 for white or black pixels.
    public  byte[] gfx = new byte[32 * 64];

    //Stack variables:
    private short[] stack = new short[16];
    private short sp = 0;

    public byte[] key = new byte[16];

    private byte[] chip8Fontset = {
        0xF0, 0x90, 0x90, 0x90, 0xF0,
        0x20, 0x60, 0x20, 0x20, 0x70,
        0xF0, 0x10, 0xF0, 0x80, 0xF0,
        0xF0, 0x10, 0xF0, 0x10, 0xF0,
        0x90, 0x90, 0xF0, 0x10, 0x10,
        0xF0, 0x80, 0xF0, 0x10, 0xF0,
        0xF0, 0x80, 0xF0, 0x90, 0xF0,
        0xF0, 0x10, 0x20, 0x40, 0x40,
        0xF0, 0x90, 0xF0, 0x90, 0xF0,
        0xF0, 0x90, 0xF0, 0x10, 0xF0,
        0xF0, 0x90, 0xF0, 0x90, 0x90,
        0xE0, 0x90, 0xE0, 0x90, 0xE0,
        0xF0, 0x80, 0x80, 0x80, 0xF0,
        0xE0, 0x90, 0x90, 0x90, 0xE0,
        0xF0, 0x80, 0xF0, 0x80, 0xF0,
        0xF0, 0x80, 0xF0, 0x80, 0x80
    };

    private Random rand = new Random();

    public void Initialize() {
        //Loads fontset into memory.
        for (int i = 0; i < chip8Fontset.Length; i++) {
            this.memory[i + 50] = chip8Fontset[i];
        }
    }

    public void LoadGame(byte[] rom) {
        for (int i = 0; i < rom.Length; i++) {
            memory[i + 512] = rom[i];
        }
    }

    public void EmulateCycle() {
        //Fetch the opcode from memory using the current memory location stored in pc.
        opcode = (short)(memory[pc] << 8 | memory[pc + 1]);

        string code = opcode.ToString("X4");

        //Grab generally needed values from the opcode.
        int x = (opcode & 0x0F00) >> 8;
        int y = (opcode & 0x00F0) >> 4;
        int n = opcode & 0x000F;
        byte nn = (byte)(opcode & 0x00FF);
        short nnn = (short)(opcode & 0x0FFF);
        int temp;
        int vx = registers[x];
        int vy = registers[y];

        switch (opcode & 0xF000)
        {
            case 0x0000:
                switch (opcode & 0x000F)
                {
                    case 0x000: //0x00E0
                        gfx = new byte[2048];
                        drawFlag = true;
                        pc += 2;

                        break;

                    case 0x00E: //0x000E
                        sp -= 1;
                        pc = stack[sp];
                        pc += 2;
                        break;

                    default:
                        Console.WriteLine("Unrecognized Opcode");
                        pc += 2;
                        break;
                } break;


            case 0x1000: //0x1NNN
                pc = nnn;
                break;

            case 0x2000: //0x2NNN
                stack[sp] = pc;
                sp += 1;
                pc = nnn;
                break;

            case 0x3000: //0x3XNN
                if (registers[x] == nn) {
                    pc += 4;
                } else {
                    pc += 2;
                }
                break;

            case 0x4000: //0x4XNN
                if (registers[x] == nn)
                {
                    pc += 2;
                }
                else
                {
                    pc += 4;
                }
                break;

            case 0x5000: //0x5XY0
                if(registers[x] == registers[y]) {
                    pc += 4;
                } else {
                    pc += 2;
                }
                break;

            case 0x6000: //0x6XNN
                registers[x] = nn;
                pc += 2;
                break;

            case 0x7000: //0x7XNN
                registers[x] += nn;
                pc += 2;
                break;

            case 0x8000:
                switch (opcode & 0x000F)
                {
                    case 0x0000: //0x8XY0
                        registers[x] = registers[y];
                        pc += 2;
                        break;

                    case 0x0001: //0x8XY1
                        registers[x] = (byte)(registers[x] | registers[y]);
                        break;

                    case 0x0002: //0x8XY2
                        registers[x] = (byte)(registers[x] & registers[y]);
                        pc += 2;
                        break;

                    case 0x0003: //0x8XY3
                        registers[x] = (byte)(registers[x] ^ registers[y]);
                        pc += 2;
                        break;

                    case 0x0004: //0x8XY4
                        temp = registers[x] + registers[y];
                        registers[x] = (byte)(registers[x] + registers[y]);
                        if (temp > 255) {
                            registers[0xF] = 1; //Carry
                        } else {
                            registers[0xF] = 0; //No Carry
                        }
                        pc += 2;
                        break;

                    case 0x0005: //0x8XY5
                        temp = registers[x] - registers[y];
                        registers[x] += registers[y];
                        if (temp < 0) {
                            registers[0xF] = 0;
                        } else
                        {
                            registers[0xF] = 1;
                        }
                        pc += 2;
                        break;

                    case 0x0006: //0x8XY6
                        temp = registers[x] & 1;
                        registers[x] = (byte)(registers[x] >> 1);
                        registers[0xF] = (byte)temp;
                        pc += 2;
                        break;

                    case 0x0007: //0x8XY7
                        temp = registers[x] - registers[y];
                        registers[x] -= registers[y];
                        if (temp < 0) {
                            registers[0xF] = 0;
                        } else {
                            registers[0xF] = 1;
                        }
                        pc += 2;
                        break;

                    case 0x000E: //0x8XYE
                        temp = (registers[x] >> 7) & 1;
                        registers[x] = (byte)(registers[x] << 1);
                        registers[0xF] = (byte)temp;
                        pc += 2;
                        break;

                    default:
                        Console.WriteLine("Unrecognized Opcode");
                        pc += 2;
                        break;

                } break;
            case 0x9000: //0x9XY0
                if (registers[x] != registers[y]) {
                    pc += 4;
                } else {
                    pc += 2;
                }
                break;

            case 0xA000: //0xANNN
                I = nnn;
                pc += 2;
                break;

            case 0xB000: //0xBNNN
                pc = (short)(registers[0x00] + nnn);
                break;

            case 0xC000: //0xCXNN
                temp = (rand.Next(255));
                registers[x] = (byte)temp;
                pc += 2;
                break;

            case 0xD000: //0xDYN
                registers[0xF] = 0;
                x = registers[x];
                y = registers[y];

                int temp2;
                for (int i = 0; i < n; i++) {
                    temp = memory[I + i];
                    for (int j = 0; j < 8; j++) {
                        if ((temp & (0x80 >> j)) != 0) {
                            temp2 = (x + j + ((y + i) * 64));
                            if (gfx[temp2] == 1) { registers[0xF] = 1; }
                            gfx[temp2] ^= 1;
                        }
                    }
                }

                drawFlag = true;
                pc += 2;
                break;

            case 0xE000:
                //Key Input Code
                switch (opcode & 0x00FF)
                {
                    case 0x09E: //0xE29E
                        break;

                    case 0x00A1: //0xEXA1
                        break;

                    default:
                        Console.WriteLine("Unrecognized Opcode");
                        pc += 2;
                        break;
                } break;

            case 0xF000:
                switch (opcode & 0x00FF)
                {
                    case 0x0007: //0xFX07
                        registers[x] = (byte)delayTimer;
                        pc += 2;
                        break;

                    //Halting key input
                    case 0x000A: //0xFX0A
                        temp = registers[x];
                        if (key[temp] > 0) { pc += 2; }    
                        break;

                    case 0x0015: //0xFX15
                        delayTimer = registers[x];
                        pc += 2;
                        break;

                    case 0x0018:
                        soundTimer = registers[x];
                        break;

                    case 0x001E:
                        I += registers[x];
                        pc += 2;
                        break;

                    case 0x0033:
                        memory[I] = (byte)((x / 100) % 10);
                        memory[I + 1] = (byte)((x / 10) % 10);
                        memory[I + 2] = (byte)(x % 10);

                        pc += 2;
                        break;

                    case 0x0055:
                        for (int i = 0; i < x + 1; i++) {
                            memory[I + i] = registers[i];
                        }
                        pc += 2;
                        break;

                    case 0x0065:
                        for (int i = 0; i < x + 1; i++)
                        {
                            registers[i] = memory[I + i];
                        }
                        pc += 2;
                        break;

                    default:
                        Console.WriteLine("Unrecognized Opcode");
                        pc += 2;
                        break;
                } break;

            default:
                Console.WriteLine("Unrecognized Opcode");
                pc += 2;
                break;
        }
    }
    
}
